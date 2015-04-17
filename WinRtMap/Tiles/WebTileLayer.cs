using System;
using Windows.Foundation;

namespace WinRtMap.Tiles
{
	public class WebTileLayer : BaseTileLayer<WebTile>
	{
		protected override WebTile CreateNewTile(int x, int z, int y, Point location)
		{
			Uri uri = new Uri(string.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", z, x, y));
			return new WebTile(x, y, z, location, uri);
		}

		public WebTileLayer() : base(new WebTileLoader())
		{}
	}
}