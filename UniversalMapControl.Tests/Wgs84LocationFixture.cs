using Microsoft.VisualStudio.TestTools.UnitTesting;

using UniversalMapControl.Projections;

namespace UniversalMapControl.Tests
{
    [TestClass]
    public class Wgs84LocationFixture
    {
        [DataTestMethod]
        [DataRow(46.745475, 7.117338, 47.433630, 9.561795, 200311)]
        [DataRow(48.848806, 2.252364, 48.847047, 2.414612, 11877)]
        [DataRow(45.831477, 1.261540, 45.831487, 1.261659, 9.287)]
        [DataRow(36.12, -86.67, 33.94, -118.4, 2887259.95)]
        public void CalculateDistance(double latFrom, double longFrom, double latTo, double longTo, double expectedMeters)
        {
            Wgs84Location from = new Wgs84Location(latFrom, longFrom);
            double meters = from.DistanceTo(new Wgs84Location(latTo, longTo));

            Assert.AreEqual(expectedMeters, meters, 0.1);
        }
    }
}