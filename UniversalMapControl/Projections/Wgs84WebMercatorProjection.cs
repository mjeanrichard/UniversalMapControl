using System;

using Windows.Foundation;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Projections
{
	/// <summary>
	/// This is an implementation of the WebMercator Projection.
	/// It project any location (Longitude -180 .. 180 and Latitude -85.0511 .. 85.0511) to a 
	/// cartesian square (x and y -128..128). The overall size of the projected grid is 256 which 
	/// makes it easy to work with tiles that contain 265x256 pixels. 
	/// 
	/// The Point is used to represent Lat/Long Coordinates. X is Latitude, Y is Longitude.
	/// 
	/// </summary>
	public class Wgs84WebMercatorProjection : IProjection
	{
		public const double MapWidth = 256;
		public const int HalfMapWidth = 128;
		public const double LatNorthBound = 85.051128779803d;

		public Point ToCartesian(ILocation location, bool sanitize = true)
		{
			return ToCartesian(location, location.Longitude, sanitize);
		}

		/// <summary>
		/// Projects a Location to the respective cartesian coordinates.
		/// The Longitude component of the given wgs84 parameter is adjusted to be as close 
		/// as possible to the provided referenceLong even if that means using a Longitude 
		/// greater than 180.
		/// </summary>
		public Point ToCartesian(ILocation location, double referenceLong, bool sanitize = true)
		{
			double latitude = location.Latitude;
			double longitude = location.Longitude;
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

		public ILocation ToLocation(Point point, bool sanitize = true)
		{
		    double lat = (-point.Y / MapWidth) * 360;
		    double lon = (point.X / MapWidth) * 360;
			lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180)) - Math.PI / 2);

			if (sanitize)
			{
				lat = SanitizeLatitude(lat);
				lon = SanitizeLongitude(lon);
			}

			return new Wgs84Location(lat, lon);
		}

		/// <summary>
		/// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
		/// </summary>
		public double GetZoomFactor(double zoomLevel)
		{
			return Math.Pow(2, zoomLevel);
		}

		/// <summary>
		/// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
		/// </summary>
		public double GetZoomLevel(double zoomFactor)
		{
			return Math.Log(zoomFactor, 2);
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

		public Point SanitizeCartesian(Point point)
		{
			point.X = point.X % MapWidth;
			if (point.X > HalfMapWidth)
			{
				point.X -= MapWidth;
			}
			if (point.X < -HalfMapWidth)
			{
				point.X += MapWidth;
			}

			if (point.Y > HalfMapWidth)
			{
				point.Y = HalfMapWidth;
			}
			if (point.Y < -HalfMapWidth)
			{
				point.Y = -HalfMapWidth;
			}
			return point;
		}
	}
}