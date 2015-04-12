using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using WinRtMap.Projections;

namespace WinRtMap.Tiles
{
	public class TileLoader
	{
		private const int TileSize = 256;

		private static Rect GetTileIndexBounds(Map parentMap, int zoomLevel)
		{
			int xTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualWidth / TileSize) / 2d);
			int yTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualHeight / TileSize) / 2d);

			Point centerTileIndex = parentMap.ViewPortProjection.GetTileIndex(parentMap.MapCenter, zoomLevel);

			RotateTransform rotation = new RotateTransform {Angle = parentMap.Heading, CenterY = centerTileIndex.Y, CenterX = centerTileIndex.X};

			Rect rect = new Rect(new Point(centerTileIndex.X - xTileCount, centerTileIndex.Y - yTileCount), new Point(centerTileIndex.X + xTileCount, centerTileIndex.Y + yTileCount));
			Rect bounds = rotation.TransformBounds(rect);
			return bounds;
		}

		private readonly HttpClient _client = new HttpClient();
		private readonly ConcurrentBag<WebTile> _tilesToLoad = new ConcurrentBag<WebTile>();
		private volatile int _taskCount;
		private Dictionary<int, Dictionary<string, WebTile>> _tileCache = new Dictionary<int, Dictionary<string, WebTile>>();
		private Wgs84WebMercatorProjection _projection = new Wgs84WebMercatorProjection();

		public TileLoader()
		{
			ZoomLevelOffset = 0.25;
		}

		public double ZoomLevelOffset { get; set; }

		/// <summary>
		/// This method calculates all required tiles for the current Map. The function calculates the smallest axis-aligned 
		/// bounding box possible for the current ViewPort and returns the Tiles required for the calculated bounding box. 
		/// This means that if the current Map has a Heading that is not a multiple of 90° 
		/// this function will return too many tiles.
		/// </summary>
		protected virtual IEnumerable<Point> GetTileIndizes(Map parentMap, int zoomLevel)
		{
			Rect bounds = GetTileIndexBounds(parentMap, zoomLevel);
			for (int x = (int)Math.Floor(bounds.Left); x <= Math.Ceiling(bounds.Right); x++)
			{
				for (int y = (int)Math.Floor(bounds.Top); y <= Math.Ceiling(bounds.Bottom); y++)
				{
					yield return new Point(x, y);
				}
			}
		}

		public void RefreshTiles(Map parentMap)
		{
			int tileZoomLevel = (int)Math.Floor(parentMap.ZoomLevel + ZoomLevelOffset);

			Dictionary<string, WebTile> tiles;
			if (!_tileCache.TryGetValue(tileZoomLevel, out tiles))
			{
				tiles = new Dictionary<string, WebTile>();
				_tileCache.Add(tileZoomLevel, tiles);
			}

			Dictionary<string, WebTile> tilesToRemove = new Dictionary<string, WebTile>(tiles);
			foreach (Point index in GetTileIndizes(parentMap, tileZoomLevel))
			{
				int x = (int)index.X;
				int y = parentMap.ViewPortProjection.SanitizeIndex((int)Math.Round(index.Y), tileZoomLevel);

				string key = string.Join("/", x, y, tileZoomLevel);

				if (tiles.ContainsKey(key))
				{
					tilesToRemove.Remove(key);
					continue;
				}
				if (!tiles.ContainsKey(key))
				{
					Point position = parentMap.ViewPortProjection.GetViewPortPositionFromTileIndex(new Point(x, y), tileZoomLevel);
					Point location = parentMap.ViewPortProjection.ToWgs84(position, false);
					WebTile tile = new WebTile(x, y, tileZoomLevel, location);
					Enqueue(tile);
					tiles.Add(key, tile);
				}
			}

			foreach (var oldTile in tilesToRemove)
			{
				oldTile.Value.IsRemoved = true;
				tiles.Remove(oldTile.Key);
			}

			foreach (KeyValuePair<int, Dictionary<string, WebTile>> zoomLevelTiles in _tileCache)
			{
				if (zoomLevelTiles.Key == tileZoomLevel || !zoomLevelTiles.Value.Any())
				{
					continue;
				}
				Rect bounds = GetTileIndexBounds(parentMap, zoomLevelTiles.Key);
				List<string> keysToRemove = new List<string>();
				foreach (KeyValuePair<string, WebTile> tile in zoomLevelTiles.Value)
				{
					if (!tile.Value.HasImage || !bounds.Contains(new Point(tile.Value.X, tile.Value.Y)))
					{
						//Prevent donwloading tiles that are not on the current ZoomLevel.
						tile.Value.IsRemoved = true;
                        keysToRemove.Add(tile.Key);
					}
				}
				foreach (string key in keysToRemove)
				{
					zoomLevelTiles.Value.Remove(key);
				}
			}
		}

		protected void Enqueue(WebTile tile)
		{
			if (!tile.HasImage)
			{
				_tilesToLoad.Add(tile);
				StartDownloading();
			}
		}

		private void StartDownloading()
		{
			if (_taskCount >= 5)
			{
				return;
			}
			Interlocked.Increment(ref _taskCount);

			Task.Run(async () =>
			{
				WebTile tile;
				while (_tilesToLoad.TryTake(out tile))
				{
					if (tile.HasImage || tile.IsRemoved)
					{
						continue;
					}
					try
					{
						int x = _projection.SanitizeIndex(tile.X, tile.Zoom);
						int y = _projection.SanitizeIndex(tile.Y, tile.Zoom);
						Uri uri = new Uri(string.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", tile.Zoom, x, y));
                        using (HttpResponseMessage response = await _client.GetAsync(uri))
						{
							using (MemoryStream memStream = new MemoryStream())
							{
								await response.Content.CopyToAsync(memStream);
								memStream.Position = 0;
								using (IRandomAccessStream ras = memStream.AsRandomAccessStream())
								{
									await tile.SetImage(ras);
								}
							}
						}
					}
					catch (Exception e)
					{
						//If one Tile could not be donwloaded continue with the next.
						//TODO: Implement some proper Error Handling here. Retry?
					}
				}
				Interlocked.Decrement(ref _taskCount);
			});
		}

		public IEnumerable<BaseTile> GetTiles(double zoomLevel)
		{
			int tileZoomLevel = (int)Math.Floor(zoomLevel + ZoomLevelOffset);

			foreach (KeyValuePair<int, Dictionary<string, WebTile>> tileLayer in _tileCache.OrderBy(t => t.Key))
			{
				if (tileLayer.Key > tileZoomLevel)
				{ 
					continue;
				}
				foreach (WebTile tile in tileLayer.Value.Values)
				{
					yield return tile;
				}
			}

		}
	}
}