using System;
using System.Globalization;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Projections
{
    public class Wgs84Location : ILocation
    {
        private const int EarthRadiusMeter = 6372800;

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

        public string ToString(string format)
        {
            return string.Format(CultureInfo.CurrentCulture, format, Latitude, Longitude);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}, {1}", Latitude, Longitude);
        }

        public double DistanceTo(ILocation to)
        {
            double deltaLong = DegToRad(to.Longitude - Longitude);
            double deltaLat = DegToRad(to.Latitude - Latitude);

            double latFrom = DegToRad(Latitude);
            double latTo = DegToRad(to.Latitude);

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) + Math.Cos(latFrom) * Math.Cos(latTo) * (Math.Sin(deltaLong / 2) * Math.Sin(deltaLong / 2));
            double angle = 2 * Math.Asin(Math.Sqrt(a));
            return EarthRadiusMeter * 2 * Math.Asin(Math.Sqrt(a));
        }

        private static double DegToRad(double deg)
        {
            return Math.PI * deg / 180.0;
        }
    }
}