using System;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Projections;

namespace UniversalMapControl.Tests
{
	[TestClass]
	public class SwissGridFixture
	{
		[DataTestMethod]
		[DataRow(634921, 244309, 47.348712771, 7.900784851)]
		public void CanConvertToLocation(int x, int y, double latitude, double longitude)
		{
			SwissGridLocation sg = new SwissGridLocation(x, y);
			ILocation location = sg.ToWgs84Approx();

			if (Math.Abs(latitude - location.Latitude) > 0.000003)
			{
				Assert.Fail("Expected Value {0} for Latitude is not Eual to actual Value {1}. (Diff: {2})", latitude, location.Latitude, Math.Abs(latitude - location.Latitude));
			}
			if (Math.Abs(longitude - location.Longitude) > 0.00001)
			{
				Assert.Fail("Expected Value {0} for Longitude is not Eual to actual Value {1}. (Diff: {2})", longitude, location.Longitude, Math.Abs(longitude - location.Longitude));
			}
		}

		[DataTestMethod]
		[DataRow(634921, 244309)]
		[DataRow(634000, 244000)]
		[DataRow(600001, 200001)]
		public void CanRoundtrip(int x, int y)
		{
			SwissGridLocation expected = new SwissGridLocation(x, y);
			ILocation location = expected.ToWgs84Approx();
			SwissGridLocation actual = SwissGridLocation.FromWgs84Approx(location);

			Assert.AreEqual(expected.X, actual.X);
			Assert.AreEqual(expected.Y, actual.Y);
		}
	}
}