using System;
using System.Numerics;

using Windows.UI.Xaml.Media;

namespace UniversalMapControl.Utils
{
	public static class TransformHelper
	{
		private const double RadFactor = Math.PI / 180.0;

		public static Matrix ToXamlMatrix(this Matrix3x2 m)
		{
			return new Matrix(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
		}

		public static Matrix3x2 ToMatrix(this Matrix m)
		{
			return new Matrix3x2((float)m.M11, (float)m.M12, (float)m.M21, (float)m.M22, (float)m.OffsetX, (float)m.OffsetY);
		}

		public static float DegToRad(double degrees)
		{
			return (float)(degrees * RadFactor);
		}
	}
}