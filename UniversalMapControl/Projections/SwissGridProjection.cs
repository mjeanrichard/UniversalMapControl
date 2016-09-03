using System;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Projections
{
    public class SwissGridProjection : IProjection
    {
        public int MaxZoomLevel = 17;

        public ILocation ToLocation(CartesianPoint point, bool sanitize = true)
        {
            if (sanitize)
            {
                point = SanitizeCartesian(point);
            }
            double x = (point.X + 42000000) / 100d;
            double y = (35000000 - point.Y) / 100d;

            return new SwissGridLocation(x, y);
        }

        public CartesianPoint ToCartesian(ILocation location, bool sanitize = true)
        {
            if (location == null)
            {
                return new CartesianPoint();
            }
            SwissGridLocation sgLocation = GetSwissGridLocation(location);
            int x = (int)Math.Round(sgLocation.X * 100) - 42000000;
            int y = 35000000 - (int)Math.Round(sgLocation.Y * 100);

            if (sanitize)
            {
                return SanitizeCartesian(new CartesianPoint(x, y));
            }
            return new CartesianPoint(x, y);
        }

        public CartesianPoint ToCartesian(ILocation location, double referenceLong, bool sanitize = true)
        {
            return ToCartesian(location, sanitize);
        }

        public CartesianPoint SanitizeCartesian(CartesianPoint value)
        {
            long x = Math.Max(0, Math.Min(48000000, value.X));
            long y = Math.Max(0, Math.Min(32000000, value.Y));
            return new CartesianPoint(x, y);
        }

        /// <summary>
        /// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
        /// </summary>
        public double GetZoomFactor(double zoomLevel)
        {
            zoomLevel = Math.Min(zoomLevel, MaxZoomLevel);
            return 1 / Math.Pow(2, MaxZoomLevel - zoomLevel);
        }

        /// <summary>
        /// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
        /// </summary>
        public double GetZoomLevel(double zoomFactor)
        {
            double log = Math.Log(1 / zoomFactor, 2);
            return Math.Min(MaxZoomLevel - log, MaxZoomLevel);
        }

        public double CartesianScaleFactor(ILocation center)
        {
            // Scale Factor is the same everywhere in SwissGrid.
            return 100;
        }

        private SwissGridLocation GetSwissGridLocation(ILocation location)
        {
            SwissGridLocation sgLocation = location as SwissGridLocation;
            if (sgLocation != null)
            {
                return sgLocation;
            }
            return SwissGridLocation.FromWgs84Approx(location);
        }
    }
}