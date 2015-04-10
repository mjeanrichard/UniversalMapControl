using System;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WinRtMap.Projections;

namespace WinRtMap
{
	public class Map : MapLayerBase
	{
		public static readonly DependencyProperty MapCenterProperty = DependencyProperty.Register(
			"MapCenter", typeof(Point), typeof(Map), new PropertyMetadata(new Point(), MapCenterChanged));

		public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
			"Heading", typeof(double), typeof(Map), new PropertyMetadata(0d, HeadingChanged));

		public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
			"ZoomLevel", typeof(double), typeof(Map), new PropertyMetadata(0d, ZoomLevelChanged));

		private static void HeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapHeadingChanged((double)e.NewValue);
		}

		private static void MapCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapCenterChanged((Point)e.NewValue);
		}

		private static void ZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnZoomLevelChanged((double)e.NewValue);
		}

		public event EventHandler<Point> MapCenterChangedEvent;
		public event EventHandler<double> MapHeadingChangedEvent;
		public event EventHandler ViewPortChangedEvent;
		public event EventHandler<double> ZoomLevelChangedEvent;
		public readonly Wgs84WebMercatorProjection ViewPortProjection = new Wgs84WebMercatorProjection();
		private double _headingBeforeManipulation;
		private Point _manipulationStartPoint;
		private Point _viewPortCenter;
		private Point _viewPortCenterBeforeManipulation;
		private double _zoomBeforeManipulation;

		public Map()
		{
			ViewPortTransform = new TranslateTransform();

			ScaleTransform = new ScaleTransform();
			RotateTransform = new RotateTransform();
			ScaleRotateTransform = new TransformGroup {Children = {ScaleTransform, RotateTransform}};

			ZoomLevel = 1;
			SizeChanged += Map_SizeChanged;

			ManipulationMode = ManipulationModes.All;
			ManipulationStarted += OnManipulationStarted;
			ManipulationCompleted += OnManipulationCompleted;
			ManipulationDelta += OnManipulationDelta;

			MapCenter = new Point(0, 0);
		}

		public Point MapCenter
		{
			get { return (Point)GetValue(MapCenterProperty); }
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
		public ScaleTransform ScaleTransform { get; }
		public RotateTransform RotateTransform { get; }
		public TransformGroup ScaleRotateTransform { get; }

		public TranslateTransform ViewPortTranslation { get; set; }

		public Point ViewPortCenter
		{
			get { return _viewPortCenter; }
			private set
			{
				if (_viewPortCenter != value)
				{
					_viewPortCenter = value;
					MapCenter = ViewPortProjection.ToWgs84(value);
				}
			}
		}

		protected override Map GetParentMap()
		{
			return this;
		}

		private void Map_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateViewPortTransform();
		}

		protected virtual void OnMapHeadingChanged(double newHeading)
		{
			EventHandler<double> mapHeadingChangedEvent = MapHeadingChangedEvent;
			if (mapHeadingChangedEvent != null)
			{
				mapHeadingChangedEvent(this, newHeading);
			}
			_viewPortCenter = ViewPortProjection.ToCartesian(MapCenter);
			UpdateViewPortTransform();
		}

		protected virtual void OnViewPortChangedEvent()
		{
			EventHandler viewPortChangedEvent = ViewPortChangedEvent;
			if (viewPortChangedEvent != null)
			{
				viewPortChangedEvent(this, EventArgs.Empty);
			}
		}

		protected virtual void OnZoomLevelChanged(double newZoomLevel)
		{
			EventHandler<double> zoomLevelChangedEvent = ZoomLevelChangedEvent;
			if (zoomLevelChangedEvent != null)
			{
				zoomLevelChangedEvent(this, newZoomLevel);
			}
			UpdateViewPortTransform();
		}

		/// <summary>
		/// Calculates the ViewPortTransfrom for the Current Heading and ViewPortCenter. 
		/// Invalidates the ViewPort to schedule a redraw.
		/// </summary>
		protected virtual void UpdateViewPortTransform()
		{
			double centerX = ViewPortCenter.X;
			double centerY = ViewPortCenter.Y;
			double scaleFactor = GetScaleFactor(ZoomLevel);
			double dx = centerX - (ActualWidth / 2);
			double dy = centerY - (ActualHeight / 2);

			ScaleTransform viewPortScale = new ScaleTransform {ScaleY = scaleFactor, ScaleX = scaleFactor, CenterX = centerX, CenterY = centerY};
			RotateTransform viewPortRotation = new RotateTransform {Angle = Heading, CenterX = centerX, CenterY = centerY};
			TranslateTransform viewPortTranslation = new TranslateTransform {X = -dx, Y = -dy};

			ScaleTransform.ScaleX = scaleFactor;
			ScaleTransform.ScaleY = scaleFactor;
			RotateTransform.Angle = Heading;

			TransformGroup transform = new TransformGroup();
			transform.Children.Add(viewPortScale);
			transform.Children.Add(viewPortRotation);
			transform.Children.Add(viewPortTranslation);
			ViewPortTransform = transform;
			InvalidateArrange();
			OnViewPortChangedEvent();
		}

		#region Manipulation

		private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			UpdateManipulation(e.Cumulative);
			e.Handled = true;
		}

		private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			UpdateManipulation(e.Cumulative);
			e.Handled = true;
		}

		private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
		{
			_headingBeforeManipulation = Heading;
			_zoomBeforeManipulation = ZoomLevel;
			_viewPortCenterBeforeManipulation = ViewPortCenter;
			_manipulationStartPoint = ViewPortTransform.Inverse.TransformPoint(e.Position);
			e.Handled = true;
		}

		protected virtual void OnMapCenterChanged(Point newCenter)
		{
			EventHandler<Point> mapCenterChangedEvent = MapCenterChangedEvent;
			if (mapCenterChangedEvent != null)
			{
				mapCenterChangedEvent(this, newCenter);
			}
			_viewPortCenter = ViewPortProjection.ToCartesian(MapCenter);
			UpdateViewPortTransform();
		}

		/// <summary>
		/// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
		/// This should probably not be implemented here (Projection?).
		/// </summary>
		public double GetScaleFactor(double zoom)
		{
			return Math.Pow(2, zoom);
		}

		protected virtual void UpdateManipulation(ManipulationDelta delta)
		{
			double newZoomLevel = _zoomBeforeManipulation + Math.Log(delta.Scale, 2);
			double newHeading = _headingBeforeManipulation;

			TransformGroup transform = new TransformGroup();

			double translationScaleFactor = 1 / GetScaleFactor(_zoomBeforeManipulation);
			TranslateTransform translate = new TranslateTransform {X = -delta.Translation.X * translationScaleFactor, Y = -delta.Translation.Y * translationScaleFactor};
			transform.Children.Add(translate);

			//Revert current Rotation of the Map (this Rotation was centered around the original ViewPortCenter)
			RotateTransform mapRotation = new RotateTransform {Angle = -_headingBeforeManipulation, CenterX = _viewPortCenterBeforeManipulation.X, CenterY = _viewPortCenterBeforeManipulation.Y};
			transform.Children.Add(mapRotation);

			if (delta.Rotation != 0)
			{
				//Add the Rotation from the Manipulation
				RotateTransform manipulationRotation = new RotateTransform {Angle = -delta.Rotation, CenterX = _manipulationStartPoint.X, CenterY = _manipulationStartPoint.Y};
				transform.Children.Add(manipulationRotation);
				newHeading = (_headingBeforeManipulation + delta.Rotation) % 360;
				if (newHeading < 0)
				{
					newHeading += 360;
				}
			}

			double scaleFactor = GetScaleFactor(_zoomBeforeManipulation - newZoomLevel);
			Transform scale = new ScaleTransform {ScaleX = scaleFactor, ScaleY = scaleFactor, CenterX = _manipulationStartPoint.X, CenterY = _manipulationStartPoint.Y};
			transform.Children.Add(scale);

			Heading = newHeading;
			ZoomLevel = newZoomLevel;

			ViewPortCenter = transform.TransformPoint(_viewPortCenterBeforeManipulation);
			UpdateViewPortTransform();
		}

		#endregion
	}
}