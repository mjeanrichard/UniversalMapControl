using System;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Projections
{
	public class SwissGridProjection : IProjection
	{
		public ILocation ToLocation(CartesianPoint point, bool sanitize = true)
		{
			if (sanitize)
			{
				point = SanitizeCartesian(point);
			}
			int lon = (int)(point.X + 420000);
			int lat = (int)(350000 - point.Y);

			return new SwissGridLocation(lon, lat);
		}

		public CartesianPoint ToCartesian(ILocation location, bool sanitize = true)
		{
			int x = (int)Math.Round(location.Longitude) - 420000;
			int y = 350000 - (int)Math.Round(location.Latitude);

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
			long x = Math.Max(0, Math.Min(480000, value.X));
			long y = Math.Max(0, Math.Min(320000, value.Y));
			return new CartesianPoint(x, y);
		}

		/// <summary>
		/// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
		/// </summary>
		public double GetZoomFactor(double zoomLevel)
		{
			return Math.Pow(2, zoomLevel) / 33554432;
		}

		/// <summary>
		/// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
		/// </summary>
		public double GetZoomLevel(double zoomFactor)
		{
			double log = Math.Log(zoomFactor, 2);
			return  log;
		}

	}
}