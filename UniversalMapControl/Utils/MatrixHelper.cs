using System;
using System.Numerics;

using Windows.UI.Xaml.Media;

namespace UniversalMapControl.Utils
{
	public static class TransformHelper
	{
		private const double RadFactor = Math.PI / 180.0;

		public static Matrix ToXamlMatrix(this MatrixDouble m)
		{
			return new Matrix(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
		}

		public static MatrixDouble ToMatrix(this Matrix m)
		{
			return new MatrixDouble(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY);
		}

		public static double DegToRad(double degrees)
		{
			return degrees * RadFactor;
		}
	}
}