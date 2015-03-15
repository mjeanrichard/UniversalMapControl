using Windows.Foundation;

namespace WinRtMap.Utils
{
	public struct Location
	{
		private readonly double _latitude;
		private readonly double _longitude;

		public Location(double longitude, double latitude)
		{
			_longitude = longitude;
			_latitude = latitude;
		}

		public double Longitude
		{
			get { return _longitude; }
		}

		public double Latitude
		{
			get { return _latitude; }
		}

		public bool Equals(Location other)
		{
			return _longitude.Equals(other._longitude) && _latitude.Equals(other._latitude);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is Location && Equals((Location)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_longitude.GetHashCode() * 397) ^ _latitude.GetHashCode();
			}
		}

		public Point ToPoint()
		{
			return new Point(_longitude, _latitude);
		}
	}

	public struct ViewPortPoint
	{
		private readonly double _x;
		private readonly double _y;

		public ViewPortPoint(double x, double y)
		{
			_x = x;
			_y = y;
		}

		public ViewPortPoint(Point point)
		{
			_x = point.X;
			_y = point.Y;
		}

		public double X
		{
			get { return _x; }
		}

		public double Y
		{
			get { return _y; }
		}

		public Point ToPoint()
		{
			return new Point(_x, _y);
		}

		public bool Equals(ViewPortPoint other)
		{
			return _x.Equals(other._x) && _y.Equals(other._y);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is ViewPortPoint && Equals((ViewPortPoint)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_x.GetHashCode() * 397) ^ _y.GetHashCode();
			}
		}
	}
}