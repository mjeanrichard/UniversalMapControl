using System;
using System.Diagnostics;

using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles
{
	public class LayerConfiguration : ILayerConfiguration
	{
		private CanvasControl _canvas;

		public LayerConfiguration()
		{
			UrlPattern = "http://{RND-a;b;c}.tile.openstreetmap.org/{z}/{x}/{y}.png";
			LayerName = "OSM";

			TileProvider = new TileProvider(this);
			TileLoader = new WebTileLoader(this);
			TileCache = new FileSystemTileCache(this);
		}

		/// <summary>
		/// URL Patter to load the Tile from. The following Patter are supported:
		/// {x}/{y}/{z} : Coordinates
		/// {RND-a;b;c} : Randomly picks one of the supplied values (separated by semicolon)
		/// </summary>
		public string UrlPattern { get; set; }

		/// <summary>
		/// Name of the Layer. This is used to create a unique folder for the Filesystem Cache.
		/// </summary>
		public string LayerName { get; set; }

		public ITileProvider TileProvider { get; private set; }

		public ITileLoader TileLoader { get; }

		public ITileCache TileCache { get; }

		public ICanvasBitmapTile CreateTile(int x, int y, int zoom, ILocation location)
		{
			if (_canvas == null)
			{
				throw new InvalidOperationException("Canvas not yet set!");
			}
			return new CanvasBitmapTile(x, y, zoom, location, LayerName, _canvas);
		}

		public void SetCanvas(CanvasControl canvas)
		{
			_canvas = canvas;
		}
	}
}