using System;
using Windows.Foundation;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using UniversalMapControl.Projections;

namespace UniversalMapControl.Tests
{
	[TestClass]
	public class WebMercatorProjectionFixture
	{
		[DataTestMethod]
		[DataRow(0, 0, 0, 0)]
		[DataRow(180, 0, 0, -128)]
		[DataRow(-180, 0, -128, 0)]
		[DataRow(0, Wgs84WebMercatorProjection.LatNorthBound, 0, -128)]
		[DataRow(0, -Wgs84WebMercatorProjection.LatNorthBound, 0, 128)]
		[DataRow(0, 66.51326044311, 0, -64)]
		public void CanConvertToCartesian(double lon, double lat, double x, double y)
		{
			Wgs84WebMercatorProjection projection = new Wgs84WebMercatorProjection();

			Point point = projection.ToCartesian(new Location(lon, lat));
			if (Math.Abs(x - point.X) > 0.0000001)
			{
				Assert.Fail("The Value for X ({0}) is not close enough to the expected Value '{1}' ({2}).", point.X, x, Math.Abs(x - point.X));
			}
			if (Math.Abs(y - point.Y) > 0.0000001)
			{
				Assert.Fail("The Value for Y ({0}) is not close enough to the expected Value '{1}' ({2}).", point.Y, y, Math.Abs(y - point.Y));
			}
		}

		[DataTestMethod]
		[DataRow(0, -64, 0, 66.51326044311)]
		public void CanConvertToLatLong(double x, double y, double lon, double lat)
		{
			Wgs84WebMercatorProjection projection = new Wgs84WebMercatorProjection();

			Location loc = projection.ToWgs84(new Point(x, y));
			if (Math.Abs(lat - loc.Latitude) > 0.0000001)
			{
				Assert.Fail("The Value for Latitude ({0}) is not close enough to the expected Value '{1}' ({2}).", loc.Latitude, lat, Math.Abs(lat - loc.Latitude));
			}
			if (Math.Abs(lon - loc.Longitude) > 0.0000001)
			{
				Assert.Fail("The Value for Longitude ({0}) is not close enough to the expected Value '{1}' ({2}).", loc.Longitude, lon, Math.Abs(lon - loc.Longitude));
			}
		}

		[DataTestMethod]
		[DataRow(0, 0, 4)]
		[DataRow(0, 40.979898069620155, 4)]
		[DataRow(22.5, -21.943045533438166, 2)]
		[DataRow(22.5, -40.979898069620134, 4)]
		[DataRow(-56.25, -21.943045533438166, 5)]
		public void ViewPortToLocationRountrips(double lon, double lat, int zoom)
		{
			Wgs84WebMercatorProjection projection = new Wgs84WebMercatorProjection();

			Location result = projection.ToWgs84(projection.ToCartesian(new Location(lat, lon)));
			if (Math.Abs(lon - result.Longitude) > 0.0000001)
			{
				Assert.Fail("The Value for Lon ({0}) is not close enough to the expected Value ({1}).", result.Longitude, lon);
			}
			if (Math.Abs(lat - result.Latitude) > 0.0000001)
			{
				Assert.Fail("The Value for Lat ({0}) is not close enough to the expected Value ({1}).", result.Latitude, lat);
			}
		}
	}
}