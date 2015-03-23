using System;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WinRtMap.Projections;
using WinRtMap.Utils;

namespace WinRtMap
{
	public class Map : MapLayerBase
	{
		public static readonly DependencyProperty MapCenterProperty = DependencyProperty.Register(
			"MapCenter", typeof(Location), typeof(Map), new PropertyMetadata(new Location()));

		public readonly Wgs84WebMercatorProjection ViewPortProjection = new Wgs84WebMercatorProjection();
		private Point _initialManipulationPosition;
		private float _rotation;
		private float _rotationBeforeManipulation;
		private Point _viewPortCenter;
		private Point _viewPortCenterBeforeManipulation;
		private Matrix _viewPortMatrix;
		private Matrix _viewPortMatrixBeforeManipulation;
		private Rect _visibleMapWindow;

		public Map()
		{
			_viewPortMatrix = Matrix.Identity;

			SizeChanged += Map_SizeChanged;

			ManipulationMode = ManipulationModes.All;
			ManipulationStarted += OnManipulationStarted;
			ManipulationCompleted += OnManipulationCompleted;
			ManipulationDelta += OnManipulationDelta;

			MapCenter = new Location(0, 0);
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

		public Rect VisibleMapWindow
		{
			get { return _visibleMapWindow; }
		}

		public Location MapCenter
		{
			get { return (Location)GetValue(MapCenterProperty); }
			set
			{
				SetValue(MapCenterProperty, value);
				OnMapCenterChanged(value);
				UpdateViewPort();
			}
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

		private void UpdateViewPort()
		{
			Point viewPortCenter = ViewPortProjection.ToViewPortPoint(MapCenter, 5);
			double dx = viewPortCenter.X - (ActualWidth / 2);
			double dy = viewPortCenter.Y - (ActualHeight / 2);
			_viewPortMatrix = Matrix.Identity.RotateAt(_rotation, viewPortCenter).Translate(-dx, -dy);
			InvalidateArrange();
		}

		public event EventHandler<Location> MapCenterChangedEvent;

		protected virtual void OnMapCenterChanged(Location newCenter)
		{
			EventHandler<Location> mapCenterChangedEvent = MapCenterChangedEvent;
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
			Matrix matrix = Matrix.Identity.RotateAt(_rotation, _viewPortCenterBeforeManipulation).Translate(delta.Translation.X, delta.Translation.Y).Invert();
			Point transformedPoint = matrix.Transform(_viewPortCenterBeforeManipulation);
			MapCenter = ViewPortProjection.FromViewPortPoint(transformedPoint, 5);
		}

		private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
		{
			_rotationBeforeManipulation = Rotation;
			_viewPortMatrixBeforeManipulation = _viewPortMatrix;
			_viewPortCenterBeforeManipulation = ViewPortProjection.ToViewPortPoint(MapCenter, 5);
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