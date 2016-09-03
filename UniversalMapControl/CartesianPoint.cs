using System;
using System.Numerics;

using Windows.Foundation;

namespace UniversalMapControl
{
    public struct CartesianPoint : IEquatable<CartesianPoint>
    {
        public CartesianPoint(long x, long y)
        {
            X = x;
            Y = y;
        }

        public CartesianPoint(Point point) : this()
        {
            X = (long)Math.Round(point.X);
            Y = (long)Math.Round(point.Y);
        }

        public CartesianPoint(Vector2 vector)
        {
            X = (long)Math.Round(vector.X);
            Y = (long)Math.Round(vector.Y);
        }

        public long X { get; set; }
        public long Y { get; set; }

        public static bool operator ==(CartesianPoint left, CartesianPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CartesianPoint left, CartesianPoint right)
        {
            return !left.Equals(right);
        }

        public bool Equals(CartesianPoint other)
        {
            return (X == other.X) && (Y == other.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is CartesianPoint && Equals((CartesianPoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)((X * 397L) ^ Y);
            }
        }

        public Vector2 ToVector()
        {
            return new Vector2(X, Y);
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }
    }
}