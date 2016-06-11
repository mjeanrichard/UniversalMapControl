using System;

using Windows.Foundation;

using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles.SwissTopo
{
	public class SwissTopoTiler : ITiler
	{
		private static readonly int[] TileSizeMeter = { 102400000, 96000000, 89600000, 83200000, 76800000, 70400000, 64000000, 57600000, 51200000, 44800000, 38400000, 32000000, 25600000, 19200000, 16640000, 12800000, 6400000, 2560000, 1280000, 512000, 256000, 128000, 64000, 51200, 38400, 25600, 12800, 6400};

		private readonly Random _random = new Random();

		public int GetTileSetForZoomFactor(double zoomFactor)
		{
			double tmp = (256 / zoomFactor) * 0.9;
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
			if (point.X < 0 || point.X > 48000000)
			{
				return false;
			}
			if (point.Y < 0 || point.Y > 32000000)
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

			int r = _random.Next(5, 9);

			string url;
			if (tile.TileSet >= 24)
			{
				url = string.Format("http://wmts{3}.geo.admin.ch/1.0.0/ch.swisstopo.swisstlm3d-karte-farbe/default/current/21781/{2}/{1}/{0}.png", x, y, tile.TileSet, r);
			}
			else
			{
				url = string.Format("http://wmts{3}.geo.admin.ch/1.0.0/ch.swisstopo.pixelkarte-farbe/default/current/21781/{2}/{1}/{0}.jpeg", x, y, tile.TileSet, r);
			}
			return new Uri(url);
		}
	}
}