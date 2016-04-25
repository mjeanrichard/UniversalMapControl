using System;
using System.Collections.Generic;
using System.Linq;

using Windows.Foundation;

using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Projections;

namespace UniversalMapControl.Tiles
{
	/// <summary>
	/// Provides the nessecary Tile to a TileLayer. This class creates the required tiles for the current ViewPort and disposes
	/// of Tiles that are not required anymore.
	/// 
	/// </summary>
	public class TileProvider : ITileProvider
	{
		private readonly ITiler _tiler;
		private readonly ITileLoader _tileLoader;

		private Dictionary<int, Dictionary<string, ICanvasBitmapTile>> _tileCache = new Dictionary<int, Dictionary<string, ICanvasBitmapTile>>();
		private CanvasControl _canvas;

		public TileProvider(ITiler tiler, ITileLoader tileLoader)
		{
			_tiler = tiler;
			_tileLoader = tileLoader;
			LowerTileSetsToLoad = 0;
		}

		/// <summary>
		/// Specifies how many lower zoom level should automatically be loaded.
		/// Use 0 to disable loading of lower layers, use int.MaxValue to load all lower levels.
		/// Default is int.MaxValue.
		/// </summary>
		public int LowerTileSetsToLoad { get; set; }

		public virtual void RefreshTiles(Map parentMap)
		{
			if (_canvas == null)
			{
				return;
			}

			double zoomFactor = parentMap.ViewPortProjection.GetZoomFactor(parentMap.ZoomLevel);
			int currentTileSet = _tiler.GetTileSetForZoomFactor(zoomFactor);

			Rect bounds = parentMap.GetViewportBounds();
			CartesianPoint topLeftTile = _tiler.GetTilePositionForPoint(new CartesianPoint((long)bounds.Left, (long)bounds.Top), currentTileSet);

			int startLevel = Math.Max(0, currentTileSet - LowerTileSetsToLoad);
			for (int tileSet = startLevel; tileSet <= currentTileSet; tileSet++)
			{
				Dictionary<string, ICanvasBitmapTile> tiles;
				if (!_tileCache.TryGetValue(tileSet, out tiles))
				{
					tiles = new Dictionary<string, ICanvasBitmapTile>();
					_tileCache.Add(tileSet, tiles);
				}
				Dictionary<string, ICanvasBitmapTile> tilesToRemove = new Dictionary<string, ICanvasBitmapTile>(tiles);

				Size tileSize = _tiler.GetTileSize(currentTileSet);
				CartesianPoint tilePos = new CartesianPoint(topLeftTile.X, topLeftTile.Y);
				while (true)
				{
					if (_tiler.IsPointOnValidTile(tilePos, tileSet))
					{
						string key = string.Join("/", tilePos.X, tilePos.Y, tileSet);

						ICanvasBitmapTile tile;
						if (tiles.TryGetValue(key, out tile))
						{
							tilesToRemove.Remove(key);
						}
						if (tile == null || tile.IsDisposed)
						{
							Rect tileBounds = new Rect(new Point(tilePos.X, tilePos.Y), tileSize);
							tile = _tiler.CreateTile(tileSet, tileBounds, _canvas);

							_tileLoader.Enqueue(tile);

							tiles.Add(key, tile);
						}
					}

					tilePos.X += (long)tileSize.Width;
					if (tilePos.X > bounds.Right)
					{
						if (tilePos.Y > bounds.Bottom)
						{
							break;
						}
						tilePos.Y += (long)tileSize.Height;
						tilePos.X = topLeftTile.X;
					}
				}

				foreach (var oldTile in tilesToRemove)
				{
					tiles.Remove(oldTile.Key);
					oldTile.Value.Dispose();
				}
			}

			//Remove alle Tiles from not needed ZoomLevels
			foreach (KeyValuePair<int, Dictionary<string, ICanvasBitmapTile>> tilesPerZoom in _tileCache.Where(t => t.Key > currentTileSet).ToList())
			{
				_tileCache.Remove(tilesPerZoom.Key);
				foreach (ICanvasBitmapTile tile in tilesPerZoom.Value.Values)
				{
					tile.Dispose();
				}
			}
		}

		public virtual IEnumerable<ICanvasBitmapTile> GetTiles(double zoomFactor)
		{
			int tileSet = _tiler.GetTileSetForZoomFactor(zoomFactor);

			foreach (KeyValuePair<int, Dictionary<string, ICanvasBitmapTile>> tileLayer in _tileCache.OrderBy(t => t.Key))
			{
				if (tileLayer.Key > tileSet)
				{
					continue;
				}
				foreach (ICanvasBitmapTile tile in tileLayer.Value.Values)
				{
					yield return tile;
				}
			}
		}

		public void ResetTiles(Map parentMap, CanvasControl canvas)
		{
			_canvas = canvas;
			Dictionary<int, Dictionary<string, ICanvasBitmapTile>> tileCache = _tileCache;
			_tileCache = new Dictionary<int, Dictionary<string, ICanvasBitmapTile>>();

			foreach (Dictionary<string, ICanvasBitmapTile> layerCache in tileCache.Values)
			{
				foreach (ICanvasBitmapTile tile in layerCache.Values)
				{
					tile.Dispose();
				}
			}
			RefreshTiles(parentMap);
		}
	}
}