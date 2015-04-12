using System;
using Windows.Foundation;

namespace WinRtMap.Tiles
{
	public class WebTile : BaseTile
	{
		public WebTile(int x, int y, int zoom, Point location) : base(x, y, zoom, location)
		{}

		public Uri Uri
		{
			get { return new Uri(string.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", Zoom, X, Y)); }
		}
	}
}