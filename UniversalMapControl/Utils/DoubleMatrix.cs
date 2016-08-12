using System;
using System.Globalization;

using Windows.Foundation;

using SM = System.Math;

namespace UniversalMapControl.Utils
{
	public struct MatrixDouble : IEquatable<MatrixDouble>
	{
		public double M11;
		public double M12;

		public double M21;
		public double M22;

		public double M31;
		public double M32;


		public static MatrixDouble Identity { get; } = new MatrixDouble
			(
			1f, 0d,
			0d, 1f,
			0d, 0d
			);


		public bool IsIdentity
		{
			get
			{
				return M11 == 1f && M22 == 1f && // Check diagonal element first for early out.
									M12 == 0d &&
					   M21 == 0d &&
					   M31 == 0d && M32 == 0d;
			}
		}


		public Point Translation
		{
			get
			{
				Point ans;

				ans.X = M31;
				ans.Y = M32;

				return ans;
			}

			set
			{
				M31 = value.X;
				M32 = value.Y;
			}
		}


		public MatrixDouble(double m11, double m12,
						 double m21, double m22,
						 double m31, double m32)
		{
			M11 = m11; M12 = m12;
			M21 = m21; M22 = m22;
			M31 = m31; M32 = m32;
		}


		public static MatrixDouble CreateTranslation(Point position)
		{
			MatrixDouble result;

			result.M11 = 1.0d; result.M12 = 0.0d;
			result.M21 = 0.0d; result.M22 = 1.0d;

			result.M31 = position.X;
			result.M32 = position.Y;

			return result;
		}


		public static MatrixDouble CreateTranslation(double xPosition, double yPosition)
		{
			MatrixDouble result;

			result.M11 = 1.0d; result.M12 = 0.0d;
			result.M21 = 0.0d; result.M22 = 1.0d;

			result.M31 = xPosition;
			result.M32 = yPosition;

			return result;
		}


		public static MatrixDouble CreateScale(double xScale, double yScale)
		{
			MatrixDouble result;

			result.M11 = xScale; result.M12 = 0.0d;
			result.M21 = 0.0d; result.M22 = yScale;
			result.M31 = 0.0d; result.M32 = 0.0d;

			return result;
		}


		public static MatrixDouble CreateScale(double xScale, double yScale, Point centerPoint)
		{
			MatrixDouble result;

			double tx = centerPoint.X * (1 - xScale);
			double ty = centerPoint.Y * (1 - yScale);

			result.M11 = xScale; result.M12 = 0.0d;
			result.M21 = 0.0d; result.M22 = yScale;
			result.M31 = tx; result.M32 = ty;

			return result;
		}


		public static MatrixDouble CreateScale(Point scales)
		{
			MatrixDouble result;

			result.M11 = scales.X; result.M12 = 0.0d;
			result.M21 = 0.0d; result.M22 = scales.Y;
			result.M31 = 0.0d; result.M32 = 0.0d;

			return result;
		}


		public static MatrixDouble CreateScale(Point scales, Point centerPoint)
		{
			MatrixDouble result;

			double tx = centerPoint.X * (1 - scales.X);
			double ty = centerPoint.Y * (1 - scales.Y);

			result.M11 = scales.X; result.M12 = 0.0d;
			result.M21 = 0.0d; result.M22 = scales.Y;
			result.M31 = tx; result.M32 = ty;

			return result;
		}


		public static MatrixDouble CreateScale(double scale)
		{
			MatrixDouble result;

			result.M11 = scale; result.M12 = 0.0d;
			result.M21 = 0.0d; result.M22 = scale;
			result.M31 = 0.0d; result.M32 = 0.0d;

			return result;
		}


		public static MatrixDouble CreateScale(double scale, Point centerPoint)
		{
			MatrixDouble result;

			double tx = centerPoint.X * (1 - scale);
			double ty = centerPoint.Y * (1 - scale);

			result.M11 = scale; result.M12 = 0.0d;
			result.M21 = 0.0d; result.M22 = scale;
			result.M31 = tx; result.M32 = ty;

			return result;
		}


		public static MatrixDouble CreateSkew(double radiansX, double radiansY)
		{
			MatrixDouble result;

			double xTan = (double)SM.Tan(radiansX);
			double yTan = (double)SM.Tan(radiansY);

			result.M11 = 1.0d; result.M12 = yTan;
			result.M21 = xTan; result.M22 = 1.0d;
			result.M31 = 0.0d; result.M32 = 0.0d;

			return result;
		}


		public static MatrixDouble CreateSkew(double radiansX, double radiansY, Point centerPoint)
		{
			MatrixDouble result;

			double xTan = (double)SM.Tan(radiansX);
			double yTan = (double)SM.Tan(radiansY);

			double tx = -centerPoint.Y * xTan;
			double ty = -centerPoint.X * yTan;

			result.M11 = 1.0d; result.M12 = yTan;
			result.M21 = xTan; result.M22 = 1.0d;
			result.M31 = tx; result.M32 = ty;

			return result;
		}


		public static MatrixDouble CreateRotation(double radians)
		{
			MatrixDouble result;

			radians = (double)SM.IEEERemainder(radians, SM.PI * 2);

			double c, s;

			const double epsilon = 0.001f * (double)SM.PI / 180d;     // 0.1% of a degree

			if (radians > -epsilon && radians < epsilon)
			{
				// Exact case for zero rotation.
				c = 1;
				s = 0;
			}
			else if (radians > SM.PI / 2 - epsilon && radians < SM.PI / 2 + epsilon)
			{
				// Exact case for 90 degree rotation.
				c = 0;
				s = 1;
			}
			else if (radians < -SM.PI + epsilon || radians > SM.PI - epsilon)
			{
				// Exact case for 180 degree rotation.
				c = -1;
				s = 0;
			}
			else if (radians > -SM.PI / 2 - epsilon && radians < -SM.PI / 2 + epsilon)
			{
				// Exact case for 270 degree rotation.
				c = 0;
				s = -1;
			}
			else
			{
				// Arbitrary rotation.
				c = (double)SM.Cos(radians);
				s = (double)SM.Sin(radians);
			}

			// [  c  s ]
			// [ -s  c ]
			// [  0  0 ]
			result.M11 = c; result.M12 = s;
			result.M21 = -s; result.M22 = c;
			result.M31 = 0.0d; result.M32 = 0.0d;

			return result;
		}


		public static MatrixDouble CreateRotation(double radians, Point centerPoint)
		{
			MatrixDouble result;

			radians = (double)SM.IEEERemainder(radians, SM.PI * 2);

			double c, s;

			const double epsilon = 0.001f * (double)SM.PI / 180d;     // 0.1% of a degree

			if (radians > -epsilon && radians < epsilon)
			{
				// Exact case for zero rotation.
				c = 1;
				s = 0;
			}
			else if (radians > SM.PI / 2 - epsilon && radians < SM.PI / 2 + epsilon)
			{
				// Exact case for 90 degree rotation.
				c = 0;
				s = 1;
			}
			else if (radians < -SM.PI + epsilon || radians > SM.PI - epsilon)
			{
				// Exact case for 180 degree rotation.
				c = -1;
				s = 0;
			}
			else if (radians > -SM.PI / 2 - epsilon && radians < -SM.PI / 2 + epsilon)
			{
				// Exact case for 270 degree rotation.
				c = 0;
				s = -1;
			}
			else
			{
				// Arbitrary rotation.
				c = (double)SM.Cos(radians);
				s = (double)SM.Sin(radians);
			}

			double x = centerPoint.X * (1 - c) + centerPoint.Y * s;
			double y = centerPoint.Y * (1 - c) - centerPoint.X * s;

			// [  c  s ]
			// [ -s  c ]
			// [  x  y ]
			result.M11 = c; result.M12 = s;
			result.M21 = -s; result.M22 = c;
			result.M31 = x; result.M32 = y;

			return result;
		}

		public Point Transform(Point point)
		{
			Point result;

			result.X = point.X * M11 + point.Y * M21 + M31;
			result.Y = point.X * M12 + point.Y * M22 + M32;

			return result;

		}

		public double GetDeterminant()
		{
			// There isn't actually any such thing as a determinant for a non-square matrix,
			// but this 3x2 type is really just an optimization of a 3x3 where we happen to
			// know the rightmost column is always (0, 0, 1). So we expand to 3x3 format:
			//
			//  [ M11, M12, 0 ]
			//  [ M21, M22, 0 ]
			//  [ M31, M32, 1 ]
			//
			// Sum the diagnonal products:
			//  (M11 * M22 * 1) + (M12 * 0 * M31) + (0 * M21 * M32)
			//
			// Subtract the opposite diagonal products:
			//  (M31 * M22 * 0) + (M32 * 0 * M11) + (1 * M21 * M12)
			//
			// Collapse out the constants and oh look, this is just a 2x2 determinant!

			return (M11 * M22) - (M21 * M12);
		}


		public static bool Invert(MatrixDouble matrix, out MatrixDouble result)
		{
			double det = (matrix.M11 * matrix.M22) - (matrix.M21 * matrix.M12);

			if (SM.Abs(det) < double.Epsilon)
			{
				result = new MatrixDouble(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
				return false;
			}

			double invDet = 1.0d / det;

			result.M11 = matrix.M22 * invDet;
			result.M12 = -matrix.M12 * invDet;
			result.M21 = -matrix.M21 * invDet;
			result.M22 = matrix.M11 * invDet;
			result.M31 = (matrix.M21 * matrix.M32 - matrix.M31 * matrix.M22) * invDet;
			result.M32 = (matrix.M31 * matrix.M12 - matrix.M11 * matrix.M32) * invDet;

			return true;
		}


		public static MatrixDouble Lerp(MatrixDouble matrix1, MatrixDouble matrix2, double amount)
		{
			MatrixDouble result;

			// First row
			result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
			result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;

			// Second row
			result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
			result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;

			// Third row
			result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
			result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;

			return result;
		}


		public static MatrixDouble Negate(MatrixDouble value)
		{
			MatrixDouble result;

			result.M11 = -value.M11; result.M12 = -value.M12;
			result.M21 = -value.M21; result.M22 = -value.M22;
			result.M31 = -value.M31; result.M32 = -value.M32;

			return result;
		}


		public static MatrixDouble Add(MatrixDouble value1, MatrixDouble value2)
		{
			MatrixDouble result;

			result.M11 = value1.M11 + value2.M11; result.M12 = value1.M12 + value2.M12;
			result.M21 = value1.M21 + value2.M21; result.M22 = value1.M22 + value2.M22;
			result.M31 = value1.M31 + value2.M31; result.M32 = value1.M32 + value2.M32;

			return result;
		}


		public static MatrixDouble Subtract(MatrixDouble value1, MatrixDouble value2)
		{
			MatrixDouble result;

			result.M11 = value1.M11 - value2.M11; result.M12 = value1.M12 - value2.M12;
			result.M21 = value1.M21 - value2.M21; result.M22 = value1.M22 - value2.M22;
			result.M31 = value1.M31 - value2.M31; result.M32 = value1.M32 - value2.M32;

			return result;
		}


		public static MatrixDouble Multiply(MatrixDouble value1, MatrixDouble value2)
		{
			MatrixDouble result;

			// First row
			result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21;
			result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22;

			// Second row
			result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21;
			result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22;

			// Third row
			result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value2.M31;
			result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value2.M32;

			return result;
		}


		public static MatrixDouble Multiply(MatrixDouble value1, double value2)
		{
			MatrixDouble result;

			result.M11 = value1.M11 * value2; result.M12 = value1.M12 * value2;
			result.M21 = value1.M21 * value2; result.M22 = value1.M22 * value2;
			result.M31 = value1.M31 * value2; result.M32 = value1.M32 * value2;

			return result;
		}


		public static MatrixDouble operator -(MatrixDouble value)
		{
			MatrixDouble m;

			m.M11 = -value.M11; m.M12 = -value.M12;
			m.M21 = -value.M21; m.M22 = -value.M22;
			m.M31 = -value.M31; m.M32 = -value.M32;

			return m;
		}


		public static MatrixDouble operator +(MatrixDouble value1, MatrixDouble value2)
		{
			MatrixDouble m;

			m.M11 = value1.M11 + value2.M11; m.M12 = value1.M12 + value2.M12;
			m.M21 = value1.M21 + value2.M21; m.M22 = value1.M22 + value2.M22;
			m.M31 = value1.M31 + value2.M31; m.M32 = value1.M32 + value2.M32;

			return m;
		}


		public static MatrixDouble operator -(MatrixDouble value1, MatrixDouble value2)
		{
			MatrixDouble m;

			m.M11 = value1.M11 - value2.M11; m.M12 = value1.M12 - value2.M12;
			m.M21 = value1.M21 - value2.M21; m.M22 = value1.M22 - value2.M22;
			m.M31 = value1.M31 - value2.M31; m.M32 = value1.M32 - value2.M32;

			return m;
		}


		public static MatrixDouble operator *(MatrixDouble value1, MatrixDouble value2)
		{
			MatrixDouble m;

			// First row
			m.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21;
			m.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22;

			// Second row
			m.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21;
			m.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22;

			// Third row
			m.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value2.M31;
			m.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value2.M32;

			return m;
		}


		public static MatrixDouble operator *(MatrixDouble value1, double value2)
		{
			MatrixDouble m;

			m.M11 = value1.M11 * value2; m.M12 = value1.M12 * value2;
			m.M21 = value1.M21 * value2; m.M22 = value1.M22 * value2;
			m.M31 = value1.M31 * value2; m.M32 = value1.M32 * value2;

			return m;
		}


		public static bool operator ==(MatrixDouble value1, MatrixDouble value2)
		{
			return (value1.M11 == value2.M11 && value1.M22 == value2.M22 && // Check diagonal element first for early out.
												value1.M12 == value2.M12 &&
					value1.M21 == value2.M21 &&
					value1.M31 == value2.M31 && value1.M32 == value2.M32);
		}


		public static bool operator !=(MatrixDouble value1, MatrixDouble value2)
		{
			return (value1.M11 != value2.M11 || value1.M12 != value2.M12 ||
					value1.M21 != value2.M21 || value1.M22 != value2.M22 ||
					value1.M31 != value2.M31 || value1.M32 != value2.M32);
		}


		public bool Equals(MatrixDouble other)
		{
			return (M11 == other.M11 && M22 == other.M22 && // Check diagonal element first for early out.
										M12 == other.M12 &&
					M21 == other.M21 &&
					M31 == other.M31 && M32 == other.M32);
		}


		public override bool Equals(object obj)
		{
			if (obj is MatrixDouble)
			{
				return Equals((MatrixDouble)obj);
			}

			return false;
		}


		public override string ToString()
		{
			CultureInfo ci = CultureInfo.CurrentCulture;

			return String.Format(ci, "{{ {{M11:{0} M12:{1}}} {{M21:{2} M22:{3}}} {{M31:{4} M32:{5}}} }}",
								 M11.ToString(ci), M12.ToString(ci),
								 M21.ToString(ci), M22.ToString(ci),
								 M31.ToString(ci), M32.ToString(ci));
		}


		public override int GetHashCode()
		{
			return M11.GetHashCode() + M12.GetHashCode() +
				   M21.GetHashCode() + M22.GetHashCode() +
				   M31.GetHashCode() + M32.GetHashCode();
		}

		
	}
}
