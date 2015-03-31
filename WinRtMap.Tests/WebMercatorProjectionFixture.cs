using System;
using Windows.Foundation;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WinRtMap.Projections;
using WinRtMap.Utils;

namespace WinRtMap.Tests
{
	[TestClass]
	public class WebMercatorProjectionFixture
	{
		[DataTestMethod]
		[DataRow(0, 0, 4, 0, 0)]
		[DataRow(0, 40.979898069620155, 4, 0, -512)]
		[DataRow(22.5, -21.943045533438166, 4, 256, 256)]
		[DataRow(22.5, -40.979898069620134, 4, 256, 512)]
		[DataRow(180, 0, 5, 4096, 0)]
		[DataRow(180.439453125, 0, 5, -4086, 0)]
		[DataRow(-179.560546875, 0, 5, -4086, 0)]
		public void CanConvertToCartesian(double lon, double lat, int zoom,  double x, double y)
		{
			Wgs84WebMercatorProjection projection = new Wgs84WebMercatorProjection();

			Point point = projection.ToViewPortPoint(new Location(lon, lat), zoom);
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
		[DataRow(0, 0, 4)]
		[DataRow(0, 40.979898069620155, 4)]
		[DataRow(22.5, -21.943045533438166, 2)]
		[DataRow(22.5, -40.979898069620134, 4)]
		[DataRow(-56.25, -21.943045533438166, 5)]
		public void ViewPortToLocationRountrips(double lon, double lat, int zoom)
		{
			Wgs84WebMercatorProjection projection = new Wgs84WebMercatorProjection();

			Location result = projection.FromViewPortPoint(projection.ToViewPortPoint(new Location(lon, lat), zoom), zoom);
			if (Math.Abs(lon - result.Longitude) > 0.0000001)
			{
				Assert.Fail("The Value for Lon ({0}) is not close enough to the expected Value ({1}).", result.Longitude, lon);
			}
			if (Math.Abs(lat - result.Latitude) > 0.0000001)
			{
				Assert.Fail("The Value for Lat ({0}) is not close enough to the expected Value ({1}).", result.Latitude, lat);
			}
		}

		[DataTestMethod]
		[DataRow(0, 40, 4, 8, 6)]
		[DataRow(45, 21.94304553343818, 4, 10, 7)]
		[DataRow(33.75, -31.95216223802496, 5, 19, 19)]
		[DataRow(-67.5, -31.95216223802496, 5, 10, 19)]
		[DataRow(146.25, -40.97989806962013, 5, 29, 20)]
		[DataRow(246.25, -120.97989806962013, 5, 5, 16)]
		[DataRow(180.439453125, 0, 5, 0, 16)]
		public void TileIndexFromLocation(double lon, double lat, int zoom, int x, int y)
		{
			Wgs84WebMercatorProjection projection = new Wgs84WebMercatorProjection();

			Point tileIndex = projection.GetTileIndex(new Location(lon, lat), zoom);
			Assert.AreEqual(new Point(x, y), tileIndex);
		}

		[DataTestMethod]
		[DataRow(8, 5, 4, 0, 55.7765730186677)]
		public void LocationFromTileIndex(int x, int y, int zoom, double lon, double lat)
		{
			Wgs84WebMercatorProjection projection = new Wgs84WebMercatorProjection();

			Location result = projection.FromViewPortPoint(projection.GetViewPortPositionFromTileIndex(new Point(x, y), zoom), zoom);

			if (Math.Abs(lon - result.Longitude) > 0.0000001)
			{
				Assert.Fail("The Value for Longitude ({0}) is not close enough to the expected Value ({1}).", result.Longitude, lon);
			}
			if (Math.Abs(lat - result.Latitude) > 0.0000001)
			{
				Assert.Fail("The Value for Latitude ({0}) is not close enough to the expected Value ({1}).", result.Latitude, lat);
			}
		}

		[DataTestMethod]
		[DataRow(0, 40.979898069620155, 4)]
		[DataRow(45, 21.94304553343818, 4)]
		[DataRow(33.75, -31.95216223802496, 5)]
		public void TileIndexRoundTrip(double lon, double lat, int zoom)
		{
			Wgs84WebMercatorProjection projection = new Wgs84WebMercatorProjection();

			Location result = projection.FromViewPortPoint(projection.GetViewPortPositionFromTileIndex(projection.GetTileIndex(new Location(lon, lat), zoom), zoom), zoom);

			if (Math.Abs(lon - result.Longitude) > 0.0000001)
			{
				Assert.Fail("The Value for Longitude ({0}) is not close enough to the expected Value ({1}).", result.Longitude, lon);
			}
			if (Math.Abs(lat - result.Latitude) > 0.0000001)
			{
				Assert.Fail("The Value for Latitude ({0}) is not close enough to the expected Value ({1}).", result.Latitude, lat);
			}
		}
	}
}