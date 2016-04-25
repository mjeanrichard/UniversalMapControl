using System.Globalization;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Projections
{
	public class Wgs84Location : ILocation
	{
		public Wgs84Location()
		{
		}

		public Wgs84Location(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public double Latitude { get; private set; }
		public double Longitude { get; private set; }

		public ILocation ChangeLatitude(double newLatitude)
		{
			return new Wgs84Location(newLatitude, Longitude);
		}

		public ILocation ChangeLongitude(double newLongitude)
		{
			return new Wgs84Location(Latitude, newLongitude);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0}, {1}", Latitude, Longitude);
		}
	}

}