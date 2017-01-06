using Microsoft.VisualStudio.TestTools.UnitTesting;

using UniversalMapControl.Projections;
using UniversalMapControl.Tiles.Default;

namespace UniversalMapControl.Tests
{
    [TestClass]
    public class WebTilerFixture
    {
        [TestMethod]
        public void Test()
        {
            DefaultWebTiler tiler = new DefaultWebTiler();

            CartesianPoint point = tiler.GetTilePositionForPoint(new CartesianPoint(0, 0), 0);

            Assert.AreEqual(-Wgs84WebMercatorProjection.HalfCartesianMapWidth, point.X);
            Assert.AreEqual(-Wgs84WebMercatorProjection.HalfCartesianMapWidth, point.Y);
        }
    }
}