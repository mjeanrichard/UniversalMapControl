using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using WinRtMap.Projections;

namespace WinRtMap.Tiles
{
	public abstract class BaseTileLayer<TTile> : MapLayerBase where TTile : BaseTile
	{
		protected const int TileWidth = 256;
		private readonly BaseTileLoader<TTile> _tileLoader;
		private readonly Dictionary<int, Dictionary<string, TTile>> _tileCache = new Dictionary<int, Dictionary<string, TTile>>();

		protected BaseTileLayer(BaseTileLoader<TTile> tileLoader)
		{
			_tileLoader = tileLoader;
			ZoomLevelOffset = 0.25;
			LowerZoomLevelsToLoad = int.MaxValue;

			Loaded += TileLayer_Loaded;
		}

		public double ZoomLevelOffset { get; set; }

		/// <summary>
		/// Specifies how many lower zoom level should automatically be loaded.
		/// Use 0 to disable loading of lower layers, use int.MaxValue to load all lower levels.
		/// Default is int.MaxValue.
		/// </summary>
		public int LowerZoomLevelsToLoad { get; set; }

		private void TileLayer_Loaded(object sender, RoutedEventArgs e)
		{
			Map parentMap = GetParentMap();
			parentMap.ViewPortChangedEvent += ParentMap_ViewPortChangedEvent;
			RefreshTiles();
		}

		private void ParentMap_ViewPortChangedEvent(object sender, EventArgs e)
		{
			RefreshTiles();
		}

		/// <summary>
		/// This method calculates all required tiles for the current Map. The function calculates the smallest axis-aligned 
		/// bounding box possible for the current ViewPort and returns the Tiles required for the calculated bounding box. 
		/// This means that if the current Map has a Heading that is not a multiple of 90° 
		/// this function will return too many tiles.
		/// </summary>
		protected virtual Rect GetTileIndexBounds(Map map, Size windowSize, int zoomLevel)
		{
			double halfHeight = windowSize.Height / (TileWidth * 2);
			double halfWidth = windowSize.Width / (TileWidth * 2);

			Point centerTileIndex = GetTileIndex(map.ViewPortCenter, zoomLevel);
			Point topLeft = new Point((centerTileIndex.X - halfWidth), (centerTileIndex.Y - halfHeight));
			Point bottomRight = new Point((centerTileIndex.X + halfWidth), (centerTileIndex.Y + halfHeight));

			RotateTransform rotation = new RotateTransform {Angle = map.Heading, CenterY = centerTileIndex.Y, CenterX = centerTileIndex.X};

			Rect rect = new Rect(topLeft, bottomRight);
			Rect bounds = rotation.TransformBounds(rect);
			return bounds;
		}

		protected virtual void RefreshTiles()
		{
			Map parentMap = GetParentMap();

			int currentTileZoomLevel = (int)Math.Floor(parentMap.ZoomLevel + ZoomLevelOffset);

			Rect bounds = GetTileIndexBounds(parentMap, parentMap.RenderSize, currentTileZoomLevel);
			int startLevel = Math.Max(0, currentTileZoomLevel - LowerZoomLevelsToLoad);
			for (int z = startLevel; z <= currentTileZoomLevel; z++)
			{
				int factor = 1 << (currentTileZoomLevel - z);

				int left = (int)Math.Floor(bounds.Left / factor);
				int right = (int)Math.Ceiling(bounds.Right / factor);
				int top = (int)Math.Max(Math.Floor(bounds.Top / factor), 0);
				int maxY = (1 << z) - 1;
				int bottom = (int)Math.Min(Math.Ceiling(bounds.Bottom / factor), maxY);

				Dictionary<string, TTile> tiles;
				if (!_tileCache.TryGetValue(z, out tiles))
				{
					tiles = new Dictionary<string, TTile>();
					_tileCache.Add(z, tiles);
				}
				Dictionary<string, TTile> tilesToRemove = new Dictionary<string, TTile>(tiles);

				for (int x = left; x <= right; x++)
				{
					for (int y = top; y <= bottom; y++)
					{
						string key = string.Join("/", x, y, z);

						if (tiles.ContainsKey(key))
						{
							tilesToRemove.Remove(key);
							continue;
						}
						if (!tiles.ContainsKey(key))
						{
							Point position = GetViewPortPositionFromTileIndex(new Point(x, y), z);
							Point location = parentMap.ViewPortProjection.ToWgs84(position, false);
							int indexX = SanitizeIndex(x, z);
							TTile tile = CreateNewTile(indexX, z, y, location);

							//Check if we already have a TTile with the same Image...
							TTile sameImageTile = tiles.Values.FirstOrDefault(t => t.HasImage && t.X == tile.X && t.Y == tile.Y);
							if (sameImageTile != null)
							{
								tile.SetImage(sameImageTile);
							}
							else
							{
								_tileLoader.Enqueue(tile);
							}
							tiles.Add(key, tile);
						}
					}
				}

				foreach (var oldTile in tilesToRemove)
				{
					oldTile.Value.IsRemoved = true;
					tiles.Remove(oldTile.Key);
				}
			}

			Children.Clear();
			foreach (TTile tile in GetTiles(parentMap.ZoomLevel))
			{
				Children.Add(tile.Element);
			}
		}

		protected abstract TTile CreateNewTile(int x, int z, int y, Point location);

		protected virtual IEnumerable<TTile> GetTiles(double zoomLevel)
		{
			int tileZoomLevel = (int)Math.Floor(zoomLevel + ZoomLevelOffset);

			foreach (KeyValuePair<int, Dictionary<string, TTile>> tileLayer in _tileCache.OrderBy(t => t.Key))
			{
				if (tileLayer.Key > tileZoomLevel)
				{
					continue;
				}
				foreach (TTile tile in tileLayer.Value.Values)
				{
					yield return tile;
				}
			}
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Map parentMap = GetParentMap();
			foreach (TTile tile in GetTiles(parentMap.ZoomLevel))
			{
				Point position = parentMap.ViewPortProjection.ToCartesian(tile.Location, false);
				Point tileOrigin = parentMap.ViewPortTransform.TransformPoint(position);
				tile.Element.Arrange(new Rect((tileOrigin.X), (tileOrigin.Y), 256, 256));
				tile.UpdateTransform(parentMap.ZoomLevel, parentMap.Heading, parentMap);
			}
			return finalSize;
		}

		protected virtual int SanitizeIndex(int index, int zoom)
		{
			int tileCount = 1 << zoom;

			index = index % tileCount;
			if (index < 0)
			{
				index += tileCount;
			}
			return index;
		}

		protected virtual Point GetViewPortPositionFromTileIndex(Point tileIndex, int zoom)
		{
			int z = (1 << zoom);
			double q = Wgs84WebMercatorProjection.MapWidth / z;

			double x = (tileIndex.X * q) - Wgs84WebMercatorProjection.HalfMapWidth;
			double y = (tileIndex.Y * q) - Wgs84WebMercatorProjection.HalfMapWidth;
			return new Point(x, y);
		}

		protected virtual Point GetTileIndex(Point location, int zoom, bool sanitize = true)
		{
			int z = (1 << zoom);
			double q = Wgs84WebMercatorProjection.MapWidth / z;

			int x = (int)Math.Floor(location.X / q) - z / 2;
			int y = (int)Math.Floor(location.Y / q) + z / 2;

			if (sanitize)
			{
				return new Point(SanitizeIndex(x, zoom), SanitizeIndex(y, zoom));
			}
			return new Point(x, y);
		}
	}
}