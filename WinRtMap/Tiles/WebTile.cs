using System;
using Windows.Foundation;

namespace WinRtMap.Tiles
{
	public class WebTile : BaseTile
	{
		public WebTile(int x, int y, int zoom, Point location, Uri url) : base(x, y, zoom, location)
		{
			Url = url;
		}

		public Uri Url { get; private set; }
	}
}