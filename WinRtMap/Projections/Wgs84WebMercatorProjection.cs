using System;
using Windows.Foundation;
using WinRtMap.Utils;

namespace WinRtMap.Projections
{
	/// <summary>
	/// This is an implementation of the WebMercator Projection.
	/// It project any location (Longitude -180 .. 180 and Latitude -85.0511 .. 85.0511) to a 
	/// cartesian square (x and y -128..128). The overall size of the projected gris it 256 which 
	/// makes it easy to work with tiles that contain 265x256 pixels. 
	/// </summary>
	public class Wgs84WebMercatorProjection
	{
		private const double MapWidth = 256;
		private const int HalfMapWidth = 128;
		public const double LatNorthBound = 85.051128779803d;

		public Point ToCartesian(Location wgs84)
		{
			return ToCartesian(wgs84, wgs84.Longitude);
		}

		/// <summary>
		/// Projects a Location to the respective cartesian coordinates.
		/// The Longitude component of the given wgs84 parameter is adjusted to be as close 
		/// as possible to the provided referenceLong even if that means using a Longitude 
		/// greater than 180.
		/// </summary>
		/// <param name="wgs84"></param>
		/// <param name="referenceLong"></param>
		/// <returns></returns>
		public Point ToCartesian(Location wgs84, double referenceLong) 
		{
			double longitude = wgs84.Longitude % 360;
			if (longitude < -180)
			{
				longitude += 360;
			}
			else if (longitude > 180)
			{
				longitude -= 360;
			}

			double latitude = wgs84.Latitude;
			if (latitude > LatNorthBound)
			{
				latitude = LatNorthBound;
			}
			else if (latitude < -LatNorthBound)
			{
				latitude = -LatNorthBound;
			}

			if (referenceLong - longitude > 180)
			{
				longitude += 360;
			}
			else if (referenceLong - longitude < -180)
			{
				longitude -= 360;
			}

			double x = longitude * MapWidth / 360;
			double y = Math.Log(Math.Tan((90 + latitude) * Math.PI / 360)) / (Math.PI / 180);
			y = y * MapWidth / 360;

			return new Point(x, -y);
		}

		public Location ToWgs84(Point point)
		{
			double lon = (point.X / MapWidth) * 360;
			double lat = (-point.Y / MapWidth) * 360;
			lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180)) - Math.PI / 2);

			lon = lon % 360;
			if (lon < -180)
			{
				lon += 360;
			}
			else if (lon > 180)
			{
				lon -= 360;
			}

			return new Location(lon, lat);
		}

		public Point GetTileIndex(Location wgs84, int zoom)
		{
			int z = (1 << zoom);
			double q = MapWidth / z;

			Point viewPortPoint = ToCartesian(wgs84);

			int x = (int)Math.Floor(viewPortPoint.X / q) - z / 2;
			int y = (int)Math.Floor(viewPortPoint.Y / q) + z / 2;

			return new Point(SanitizeIndex(x, zoom), SanitizeIndex(y, zoom));
		}

		public int SanitizeIndex(int index, int zoom)
		{
			int tileCount = 1 << zoom;

			index = index % tileCount;
			if (index < 0)
			{
				index += tileCount;
			}
			return index;
		}

		public Point GetViewPortPositionFromTileIndex(Point tileIndex, int zoom)
		{
			int z = (1 << zoom);
			double q = MapWidth / z;

			double x = (tileIndex.X * q) - HalfMapWidth;
			double y = (tileIndex.Y * q) - HalfMapWidth;
			return new Point(x, y);
		}
	}
}