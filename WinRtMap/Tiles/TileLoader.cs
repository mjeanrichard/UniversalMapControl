using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using WinRtMap.Utils;

namespace WinRtMap.Tiles
{
	public class TileLoader
	{
		private const int TileSize = 256;
		private readonly HttpClient _client = new HttpClient();
		private readonly ConcurrentBag<WebTile> _tilesToLoad = new ConcurrentBag<WebTile>();
		private volatile int _taskCount = 0;
		private Dictionary<string, WebTile> _tiles = new Dictionary<string, WebTile>();

		protected virtual IEnumerable<Point> GetTileIndizes(Map parentMap)
		{
			int xTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualWidth / TileSize) / 2d);
			int yTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualHeight / TileSize) / 2d);

			Point centerTileIndex = parentMap.ViewPortProjection.GetTileIndex(parentMap.MapCenter, (int)parentMap.ZoomLevel);

			RotateTransform rotation = new RotateTransform() {Angle = parentMap.Heading, CenterY = centerTileIndex.Y, CenterX = centerTileIndex.X};

			Rect rect = new Rect(new Point(centerTileIndex.X - xTileCount, centerTileIndex.Y - yTileCount), new Point(centerTileIndex.X + xTileCount, centerTileIndex.Y + yTileCount));
			Rect bounds = rotation.TransformBounds(rect);
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
			Dictionary<string, WebTile> oldTiles = new Dictionary<string, WebTile>(_tiles);

			WebTile tile;
			foreach (Point index in GetTileIndizes(parentMap))
			{
				int x = parentMap.ViewPortProjection.SanitizeIndex((int)Math.Round(index.X), (int)parentMap.ZoomLevel);
				int y = parentMap.ViewPortProjection.SanitizeIndex((int)Math.Round(index.Y), (int)parentMap.ZoomLevel);

				string key = string.Join("/", x, y, (int)parentMap.ZoomLevel);

				if (_tiles.ContainsKey(key))
				{
					oldTiles.Remove(key);
					continue;
				}
				if (!_tiles.ContainsKey(key))
				{
					Point location = parentMap.ViewPortProjection.GetViewPortPositionFromTileIndex(new Point(x, y), (int)parentMap.ZoomLevel);
					tile = new WebTile(x, y, (int)parentMap.ZoomLevel, location);
					Enqueue(tile);
					_tiles.Add(key, tile);
				}
			}
			foreach (KeyValuePair<string, WebTile> oldTile in oldTiles)
			{
				_tiles.Remove(oldTile.Key);
				oldTile.Value.IsRemoved = true;
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
			Debug.WriteLine(_taskCount);

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
						Uri uri = tile.Uri;
						Debug.WriteLine("Downloading Tile " + tile.X + "/" + tile.Y);

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
					{}
				}
				Interlocked.Decrement(ref _taskCount);
			});
		}

		public IEnumerable<BaseTile> GetTiles()
		{
			return _tiles.Values;
		}
	}
}