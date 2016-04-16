using System;
using System.Globalization;

using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace UniversalMapControl
{
	public struct Location
	{
		public Location(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public static implicit operator Location(Point point)
		{
			return new Location(point.X, point.Y);
		}

		public static implicit operator Point(Location location)
		{
			return new Point(location.Latitude, location.Longitude);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}, {1}", Latitude, Longitude);
		}
	}
}