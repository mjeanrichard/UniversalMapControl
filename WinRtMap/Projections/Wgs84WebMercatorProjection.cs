using System;
using Windows.Foundation;

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
		private const int HalfMapWidth = 128;
		public const double LatNorthBound = 85.051128779803d;
		private const double MapWidth = 256;

		public Point ToCartesian(Point wgs84, bool sanitize = true)
		{
			return ToCartesian(wgs84, wgs84.X, sanitize);
		}

		/// <summary>
		/// Projects a Location to the respective cartesian coordinates.
		/// The Longitude component of the given wgs84 parameter is adjusted to be as close 
		/// as possible to the provided referenceLong even if that means using a Longitude 
		/// greater than 180.
		/// </summary>
		public Point ToCartesian(Point wgs84, double referenceLong, bool sanitize = true)
		{
			double longitude = wgs84.X;
			double latitude = wgs84.Y;
			if (sanitize)
			{
				longitude = SanitizeLongitude(longitude);
				latitude = SanitizeLatitude(latitude);
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

		public Point GetTileIndex(Point wgs84, int zoom)
		{
			int z = (1 << zoom);
			double q = MapWidth / z;

			Point viewPortPoint = ToCartesian(wgs84);

			int x = (int)Math.Floor(viewPortPoint.X / q) - z / 2;
			int y = (int)Math.Floor(viewPortPoint.Y / q) + z / 2;

			return new Point(SanitizeIndex(x, zoom), SanitizeIndex(y, zoom));
		}

		public Point ToWgs84(Point point, bool sanitize = true)
		{
			double lon = (point.X / MapWidth) * 360;
			double lat = (-point.Y / MapWidth) * 360;
			lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180)) - Math.PI / 2);

			if (sanitize)
			{
				lon = SanitizeLongitude(lon);
				lat = SanitizeLatitude(lat);
			}

			return new Point(lon, lat);
		}

		public Point GetViewPortPositionFromTileIndex(Point tileIndex, int zoom)
		{
			int z = (1 << zoom);
			double q = MapWidth / z;

			double x = (tileIndex.X * q) - HalfMapWidth;
			double y = (tileIndex.Y * q) - HalfMapWidth;
			return new Point(x, y);
		}

		private double SanitizeLongitude(double longitude)
		{
			longitude = longitude % 360;
			if (longitude < -180)
			{
				longitude += 360;
			}
			else if (longitude > 180)
			{
				longitude -= 360;
			}
			return longitude;
		}

		private double SanitizeLatitude(double lat)
		{
			if (lat > LatNorthBound)
			{
				lat = LatNorthBound;
			}
			else if (lat < -LatNorthBound)
			{
				lat = -LatNorthBound;
			}
			return lat;
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
	}
}