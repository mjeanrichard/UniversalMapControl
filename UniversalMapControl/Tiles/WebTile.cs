using System;
using Windows.Foundation;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace UniversalMapControl.Tiles
{
	public class WebTile : BaseTile
	{
		public WebTile(int x, int y, int zoom, Location location, Uri url, string layerName, CanvasControl canvas) : base(x, y, zoom, location, layerName, canvas)
		{
			Url = url;
		}

		public Uri Url { get; private set; }
	}
}