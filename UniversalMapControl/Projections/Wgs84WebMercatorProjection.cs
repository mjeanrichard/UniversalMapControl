using System;
using System.Globalization;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Projections
{
    /// <summary>
    /// This is an implementation of the WebMercator Projection.
    /// It project any location (Longitude -180 .. 180 and Latitude -85.0511 .. 85.0511) to a 
    /// cartesian square (x and y -128..128). The overall size of the projected grid is 256 which 
    /// makes it easy to work with tiles that contain 265x256 pixels. 
    /// </summary>
    public class Wgs84WebMercatorProjection : IProjection
    {
        public const int MaxZoomLevel = 22;
        public const long CartesianMapWidth = 256 * (1 << MaxZoomLevel);
        public const long HalfCartesianMapWidth = CartesianMapWidth / 2;
        public const double LatNorthBound = 85.051128779803d;

        private const int EquatorLengthMeters = 40075000;
        private const double EquatorMetersPerCartesianUnit = CartesianMapWidth / (double)EquatorLengthMeters;

        public CartesianPoint ToCartesian(ILocation location, bool sanitize = true)
        {
            if (location == null)
            {
                return new CartesianPoint();
            }

            return ToCartesian(location, location.Longitude, sanitize);
        }

        /// <summary>
        /// Projects a Location to the respective cartesian coordinates.
        /// The Longitude component of the given wgs84 parameter is adjusted to be as close 
        /// as possible to the provided referenceLong even if that means using a Longitude 
        /// greater than 180.
        /// </summary>
        public CartesianPoint ToCartesian(ILocation location, double referenceLong, bool sanitize = true)
        {
            if (location == null)
            {
                return new CartesianPoint();
            }

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

            long x = (long)Math.Round(longitude / 360 * CartesianMapWidth);
            double yTmp = Math.Log(Math.Tan((90 + latitude) * Math.PI / 360)) / (Math.PI / 180);
            long y = (long)Math.Round(yTmp / 360 * CartesianMapWidth);

            return new CartesianPoint(x, -y);
        }

        public ILocation ToLocation(CartesianPoint point, bool sanitize = true)
        {
            double lat = -((double)point.Y / CartesianMapWidth) * 360;
            double lon = (double)point.X / CartesianMapWidth * 360;
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
            zoomLevel = SanitizeZoomLevel(zoomLevel);
            return 1 / Math.Pow(2, MaxZoomLevel - zoomLevel);
        }

        /// <summary>
        /// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
        /// </summary>
        public double GetZoomLevel(double zoomFactor)
        {
            zoomFactor = SanitizeZoomFactor(zoomFactor);
            double log = Math.Log(1 / zoomFactor, 2);
            return SanitizeZoomLevel(MaxZoomLevel - log);
        }

        /// <summary>
        /// This Method returns a factor that can be used to scale a distance in meters to the according view port length.
        /// </summary>
        public double CartesianScaleFactor(ILocation center)
        {
            if (center == null)
            {
                return 0;
            }
            double latRad = center.Latitude * Math.PI / 180;
            return EquatorMetersPerCartesianUnit / Math.Cos(latRad);
        }

        private double SanitizeZoomFactor(double zoomFactor)
        {
            return zoomFactor <= 0 ? double.Epsilon : zoomFactor;
        }

        private double SanitizeZoomLevel(double zoomLevel)
        {
            if (zoomLevel < 0)
            {
                return 0;
            }
            if (zoomLevel > MaxZoomLevel)
            {
                return MaxZoomLevel;
            }
            return zoomLevel;
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

        public CartesianPoint SanitizeCartesian(CartesianPoint point)
        {
            point.X = point.X % CartesianMapWidth;
            if (point.X > HalfCartesianMapWidth)
            {
                point.X -= CartesianMapWidth;
            }
            if (point.X < -HalfCartesianMapWidth)
            {
                point.X += CartesianMapWidth;
            }

            if (point.Y > HalfCartesianMapWidth)
            {
                point.Y = HalfCartesianMapWidth;
            }
            if (point.Y < -HalfCartesianMapWidth)
            {
                point.Y = -HalfCartesianMapWidth;
            }
            return point;
        }

        public ILocation ParseLocation(string location)
        {
            string[] parts = location.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"'{location}' is a invalid WGS84 Location.", nameof(location));
            }
            return new Wgs84Location(double.Parse(parts[0], CultureInfo.InvariantCulture), double.Parse(parts[1], CultureInfo.InvariantCulture));
        }
    }
}