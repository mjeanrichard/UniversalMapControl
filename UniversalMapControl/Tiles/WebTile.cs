using System;
using Windows.Foundation;

namespace UniversalMapControl.Tiles
{
	public class WebTile : BaseTile
	{
		public WebTile(int x, int y, int zoom, Point location, Uri url, string layerName) : base(x, y, zoom, location, layerName)
		{
			Url = url;
		}

		public Uri Url { get; private set; }
	}
}