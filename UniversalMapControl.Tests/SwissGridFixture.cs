using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [DataTestMethod]
        [DataRow(753129, 156467, 753162, 156466, 33)]
        public void CalculateDistance(int x1, int y1, int x2, int y2, double expectedDistance)
        {
            double distance = new SwissGridLocation(x1, y1).DistanceTo(new SwissGridLocation(x2, y2));

            Assert.AreEqual(expectedDistance, distance, 0.1);
        }


        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(17)]
        public void ZoomFactorLevelRountrip(double zoomLevel)
        {
            SwissGridProjection projection = new SwissGridProjection();

            double zoomFactor = projection.GetZoomFactor(zoomLevel);
            double newZoomLevel = projection.GetZoomLevel(zoomFactor);

            Assert.AreEqual(zoomLevel, newZoomLevel, 0.001);
        }
    }
}