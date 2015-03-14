using System;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace WinRtMap
{
	public class Map : MapLayerBase
	{
		public static readonly DependencyProperty MapCenterProperty = DependencyProperty.Register(
			"MapCenter", typeof(Point), typeof(Map), new PropertyMetadata(new Point()));

		private Point _mapCenterBeforeManipulation;
		private float _rotation;
		private float _rotationBeforeManipulation;
		private Point _viewPortCenter;
		private Matrix _viewPortMatrix;
		private Matrix _viewPortMatrixBeforeManipulation;
		private Rect _visibleMapWindow;
		private Point _initialManipulationPosition;

		public Map()
		{
			_viewPortMatrix = Matrix.Identity;

			SizeChanged += Map_SizeChanged;

			ManipulationMode = ManipulationModes.All;
			ManipulationStarted += OnManipulationStarted;
			ManipulationCompleted += OnManipulationCompleted;
			ManipulationDelta += OnManipulationDelta;

			MapCenter = new Point(0, 0);
		}

		public Matrix ViewPortMatrix
		{
			get { return _viewPortMatrix; }
			set
			{
				if (!_viewPortMatrix.Equals(value))
				{
					_viewPortMatrix = value;
					InvalidateArrange();
				}
			}
		}

		public Point ViewPortCenter
		{
			get { return _viewPortCenter; }
		}

		public Rect VisibleMapWindow
		{
			get { return _visibleMapWindow; }
		}

		public Point MapCenter
		{
			get { return (Point)GetValue(MapCenterProperty); }
			set
			{
				SetValue(MapCenterProperty, value);
				OnMapCenterChanged(value);
				UpdateViewPort();
			}
		}

		private void UpdateViewPort()
		{
			Point mapCenter = MapCenter;
            double dx = mapCenter.X - (ActualWidth / 2);
			double dy = mapCenter.Y - (ActualHeight / 2);
			_viewPortMatrix = Matrix.Identity.RotateAt(_rotation, mapCenter).Translate(-dx, -dy);
			InvalidateArrange();
		}

		public float Rotation
		{
			get { return _rotation; }
			set
			{
				if (_rotation != value)
				{
					_rotation = value;
					InvalidateArrange();
				}
			}
		}

		public event EventHandler<Point> MapCenterChangedEvent;

		protected virtual void OnMapCenterChanged(Point newCenter)
		{
			EventHandler<Point> mapCenterChangedEvent = MapCenterChangedEvent;
			if (mapCenterChangedEvent != null)
			{
				mapCenterChangedEvent(this, newCenter);
			}
		}

		private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateViewPort();
		}

		private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			UpdateManipulation(e.Cumulative);
			e.Handled = true;
		}

		private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			UpdateManipulation(e.Cumulative);
			e.Handled = true;
		}

		protected virtual void UpdateManipulation(ManipulationDelta delta)
		{
			_rotation = (_rotationBeforeManipulation + delta.Rotation) % 360;
			Matrix matrix = Matrix.Identity.RotateAt(_rotation, _mapCenterBeforeManipulation).Translate(delta.Translation.X, delta.Translation.Y).Invert();
			MapCenter = matrix.Transform(_mapCenterBeforeManipulation);
		}

		private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
		{
			_rotationBeforeManipulation = Rotation;
			_viewPortMatrixBeforeManipulation = _viewPortMatrix;
			_mapCenterBeforeManipulation = MapCenter;
			_initialManipulationPosition = e.Position;
            e.Handled = true;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			return base.ArrangeOverride(finalSize);
		}

		protected override Map GetParentMap()
		{
			return this;
		}
	}
}