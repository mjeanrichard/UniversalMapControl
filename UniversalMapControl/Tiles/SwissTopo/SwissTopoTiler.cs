using System;

using Windows.Foundation;

using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles.SwissTopo
{
	public class SwissTopoTiler : ITiler
	{
		private static readonly int[] TileSizeMeter = { 1024000, 960000, 896000, 832000, 768000, 704000, 640000, 576000, 512000, 448000, 384000, 320000, 256000, 192000, 166400, 128000, 64000, 25600, 12800, 5120, 2560, 1280, 640, 512, 384, 256 };

		public int GetTileSetForZoomFactor(double zoomFactor)
		{
			double tmp = (256 / zoomFactor) * 0.8;
			for (int i = 0; i < TileSizeMeter.Length; i++)
			{
				if (tmp >= TileSizeMeter[i])
				{
					return Math.Max(i, 0);
				}
			}
			return TileSizeMeter.Length - 1;
		}

		public bool IsPointOnValidTile(CartesianPoint point, int tileSet)
		{
			if (point.X < 0 || point.X > 480000)
			{
				return false;
			}
			if (point.Y < 0 || point.Y > 320000)
			{
				return false;
			}
			return true;
		}

		public CartesianPoint GetTilePositionForPoint(CartesianPoint point, int tileSet)
		{
			long tileWidthMeters = TileSizeMeter[tileSet];

			long x = point.X / tileWidthMeters * tileWidthMeters;
			long y = point.Y / tileWidthMeters * tileWidthMeters;
			return new CartesianPoint(x, y);
		}

		public Size GetTileSize(int tileSet)
		{
			return new Size(TileSizeMeter[tileSet], TileSizeMeter[tileSet]);
		}

		public ICanvasBitmapTile CreateTile(int tileSet, Rect tileBounds, CanvasControl canvas)
		{
			return new CanvasBitmapTile(tileSet, tileBounds, canvas);
		}

		public Uri GetUrl(ICanvasBitmapTile tile)
		{
			long tileWidthMeters = TileSizeMeter[tile.TileSet];

			int x = (int)(tile.Bounds.X / tileWidthMeters);
			int y = (int)(tile.Bounds.Y / tileWidthMeters);

			string url = string.Format("http://wmts6.geo.admin.ch/1.0.0/ch.swisstopo.pixelkarte-farbe/default/20110401/21781/{2}/{1}/{0}.jpeg", x, y, tile.TileSet);
			return new Uri(url);
		}
	}
}