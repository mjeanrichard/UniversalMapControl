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
		public readonly Wgs84WebMercatorProjection ViewPortProjection = new Wgs84WebMercatorProjection();
		private double _headingBeforeManipulation;
		private Point _viewPortCenter;
		private Point _viewPortCenterBeforeManipulation;

		public Map()
		{
			ViewPortTransform = new TranslateTransform();

			ZoomLevel = 8;
			SizeChanged += Map_SizeChanged;

			ManipulationMode = ManipulationModes.All;
			ManipulationStarted += OnManipulationStarted;
			ManipulationCompleted += OnManipulationCompleted;
			ManipulationDelta += OnManipulationDelta;

			MapCenter = new Location(0, 0);
		}

		public Location MapCenter
		{
			get { return (Location)GetValue(MapCenterProperty); }
			set { SetValue(MapCenterProperty, value); }
		}

		public double Heading
		{
			get { return (double)GetValue(HeadingProperty); }
			set { SetValue(HeadingProperty, value); }
		}

		public double ZoomLevel
		{
			get { return (double)GetValue(ZoomLevelProperty); }
			set { SetValue(ZoomLevelProperty, value); }
		}

		public Transform ViewPortTransform { get; set; }

		public Point ViewPortCenter
		{
			get { return _viewPortCenter; }
			private set
			{
				if (_viewPortCenter != value)
				{
					_viewPortCenter = value;
					MapCenter = ViewPortProjection.FromViewPortPoint(value, (int)ZoomLevel);
				}
			}
		}

		protected virtual void OnViewPortChangedEvent()
		{
			EventHandler viewPortChangedEvent = ViewPortChangedEvent;
			if (viewPortChangedEvent != null)
			{
				viewPortChangedEvent(this, EventArgs.Empty);
			}
		}

		public event EventHandler<Location> MapCenterChangedEvent;
		public event EventHandler<double> MapHeadingChangedEvent;
		public event EventHandler<double> ZoomLevelChangedEvent;
		public event EventHandler ViewPortChangedEvent;

		private void UpdateMapTransform()
		{
			double dx = Math.Round(ViewPortCenter.X - (ActualWidth / 2));
			double dy = Math.Round(ViewPortCenter.Y - (ActualHeight / 2));

			TransformGroup transform = new TransformGroup();
			transform.Children.Add(new RotateTransform {Angle = Heading, CenterX = Math.Round(ViewPortCenter.X), CenterY = Math.Round(ViewPortCenter.Y)});
			transform.Children.Add(new TranslateTransform {X = -dx, Y = -dy});
			ViewPortTransform = transform;
			InvalidateArrange();
			OnViewPortChangedEvent();
		}

		protected virtual void OnMapHeadingChanged(double newHeading)
		{
			EventHandler<double> mapHeadingChangedEvent = MapHeadingChangedEvent;
			if (mapHeadingChangedEvent != null)
			{
				mapHeadingChangedEvent(this, newHeading);
			}
			_viewPortCenter = ViewPortProjection.ToViewPortPoint(MapCenter, (int)ZoomLevel);
			UpdateMapTransform();
		}

		protected virtual void OnZoomLevelChanged(double newZoomLevel)
		{
			EventHandler<double> zoomLevelChangedEvent = ZoomLevelChangedEvent;
			if (zoomLevelChangedEvent != null)
			{
				zoomLevelChangedEvent(this, newZoomLevel);
			}
			_viewPortCenter = ViewPortProjection.ToViewPortPoint(MapCenter, (int)ZoomLevel);
			UpdateMapTransform();
		}

		protected virtual void OnMapCenterChanged(Location newCenter)
		{
			EventHandler<Location> mapCenterChangedEvent = MapCenterChangedEvent;
			if (mapCenterChangedEvent != null)
			{
				mapCenterChangedEvent(this, newCenter);
			}
			_viewPortCenter = ViewPortProjection.ToViewPortPoint(MapCenter, (int)ZoomLevel);
			UpdateMapTransform();
		}

		private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateMapTransform();
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
			TransformGroup transform = new TransformGroup();
			TranslateTransform translate = new TranslateTransform{X = -delta.Translation.X, Y = -delta.Translation.Y };
			RotateTransform mapRotation = new RotateTransform{Angle = -_headingBeforeManipulation, CenterX = _viewPortCenterBeforeManipulation.X, CenterY = _viewPortCenterBeforeManipulation.Y };

			transform.Children.Add(translate);
			transform.Children.Add(mapRotation);

			if (delta.Rotation != 0)
			{
				RotateTransform manipulationRotation = new RotateTransform { Angle = -delta.Rotation, CenterX = _manipulationRotationPoint.X, CenterY = _manipulationRotationPoint.Y };
				transform.Children.Add(manipulationRotation);
				double newHeading = (_headingBeforeManipulation + delta.Rotation) % 360;
				if (newHeading < 0)
				{
					newHeading += 360;
				}
				Heading = newHeading;
			}
			ViewPortCenter = transform.TransformPoint(_viewPortCenterBeforeManipulation);
			UpdateMapTransform();
		}

		private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
		{
			_headingBeforeManipulation = Heading;
			_viewPortCenterBeforeManipulation = ViewPortCenter;
			_manipulationRotationPoint = ViewPortTransform.Inverse.TransformPoint(e.Position);
			e.Handled = true;
		}

		protected override Map GetParentMap()
		{
			return this;
		}

		private static void HeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapHeadingChanged((double)e.NewValue);
		}

		private static void ZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnZoomLevelChanged((double)e.NewValue);
		}

		private static void MapCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapCenterChanged((Location)e.NewValue);
		}

		public static readonly DependencyProperty MapCenterProperty = DependencyProperty.Register(
			"MapCenter", typeof(Location), typeof(Map), new PropertyMetadata(new Location(), MapCenterChanged));

		public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
			"Heading", typeof(double), typeof(Map), new PropertyMetadata(0d, HeadingChanged));

		public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
			"ZoomLevel", typeof(double), typeof(Map), new PropertyMetadata(0d, ZoomLevelChanged));

		private Point _manipulationRotationPoint;
	}
}