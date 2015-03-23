using System;
using Windows.Foundation;
using WinRtMap.Utils;

namespace WinRtMap.Projections
{
	public class Wgs84WebMercatorProjection
	{
		public Point ToViewPortPoint(Location wgs84, int zoom)
		{
			int zoomFactor = (1 << zoom) * 128;

			double x = wgs84.Longitude * zoomFactor / 180;
			double y = Math.Log(Math.Tan((90 + wgs84.Latitude) * Math.PI / 360)) / (Math.PI / 180);
			y = y * (zoomFactor) / 180;

			return new Point(x, -y);
		}

		public Location FromViewPortPoint(Point point, int zoom)
		{
			int zoomFactor = (1 << zoom) * 128;

			double lon = (point.X / zoomFactor) * 180;
			double lat = (point.Y / zoomFactor) * 180;
			lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180)) - Math.PI / 2);

			return new Location(lon, -lat);
		}

		public Point GetTileIndex(Location location, int zoom)
		{
			int offset = (1 << zoom) / 2;

			Point viewPortPoint = ToViewPortPoint(location, zoom);

			int x = (int)Math.Floor(viewPortPoint.X / 256) + offset;
			int y = (int)Math.Floor(viewPortPoint.Y / 256) + (offset - 1);
			return new Point(x, y);
		}

		public Point GetViewPortPositionFromTileIndex(Point tileIndex, int zoom)
		{
			int offset = (1 << zoom) / 2;

			double x = (tileIndex.X - offset) * 256;
			double y = (tileIndex.Y - (offset - 1)) * 256;
			return new Point(x, y);
		}
	}
}