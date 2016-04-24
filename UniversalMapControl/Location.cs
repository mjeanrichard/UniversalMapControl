using System.Globalization;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl
{
	public class Location : ILocation
	{
		public Location()
		{
		}

		public Location(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public double Latitude { get; private set; }
		public double Longitude { get; private set; }

		public ILocation ChangeLatitude(double newLatitude)
		{
			return new Location(newLatitude, Longitude);
		}

		public ILocation ChangeLongitude(double newLongitude)
		{
			return new Location(Latitude, newLongitude);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}, {1}", Latitude, Longitude);
		}
	}
}