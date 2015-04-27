using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WinRtMap.Projections;

namespace WinRtMap
{
	public class Map : MapLayerBase
	{
		public static readonly DependencyProperty MapCenterProperty = DependencyProperty.Register(
			"MapCenter", typeof(Point), typeof(Map), new PropertyMetadata(new Point(), MapCenterPropertyChanged));

		public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
			"Heading", typeof(double), typeof(Map), new PropertyMetadata(0d, HeadingPropertyChanged));

		public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
			"ZoomLevel", typeof(double), typeof(Map), new PropertyMetadata(0d, ZoomLevelPropertyChanged));

		private static void HeadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapHeadingChanged((double)e.NewValue);
		}

		private static void MapCenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapCenterChanged((Point)e.NewValue);
		}

		private static void ZoomLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnZoomLevelChanged((double)e.NewValue);
		}

		public event EventHandler<Point> MapCenterChangedEvent;
		public event EventHandler<double> MapHeadingChangedEvent;
		public event EventHandler ViewPortChangedEvent;
		public event EventHandler<double> ZoomLevelChangedEvent;
		public readonly Wgs84WebMercatorProjection ViewPortProjection = new Wgs84WebMercatorProjection();
		private Point _viewPortCenter;

		public Map()
		{
			ViewPortTransform = new TranslateTransform();

			ScaleTransform = new ScaleTransform();
			RotateTransform = new RotateTransform();
			TranslationTransform = new TranslateTransform();
			ScaleRotateTransform = new TransformGroup {Children = {ScaleTransform, RotateTransform}};

			MinZoomLevel = 0;
			MaxZoomLevel = 18;

			ZoomLevel = 1;
			SizeChanged += Map_SizeChanged;

			ManipulationMode = ManipulationModes.All;

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
			set
			{
				if (value < MinZoomLevel)
				{
					value = MinZoomLevel;
				}
				else if (value > MaxZoomLevel)
				{
					value = MaxZoomLevel;
				}
				SetValue(ZoomLevelProperty, value);
			}
		}

		public double MinZoomLevel { get; set; }
		public double MaxZoomLevel { get; set; }
		public Transform ViewPortTransform { get; set; }
		public ScaleTransform ScaleTransform { get; }
		public RotateTransform RotateTransform { get; }
		public TransformGroup ScaleRotateTransform { get; }
		public TranslateTransform TranslationTransform { get; set; }

		public Point ViewPortCenter
		{
			get { return _viewPortCenter; }
			set
			{
				if (_viewPortCenter != value)
				{
					_viewPortCenter = ViewPortProjection.SanitizeCartesian(value);
					MapCenter = ViewPortProjection.ToWgs84(value);
				}
			}
		}

		protected override Map LoadParentMap()
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
			double scaleFactor = ViewPortProjection.GetZoomFactor(ZoomLevel);
			double dx = centerX - (ActualWidth / 2);
			double dy = centerY - (ActualHeight / 2);

			ScaleTransform viewPortScale = new ScaleTransform {ScaleY = scaleFactor, ScaleX = scaleFactor, CenterX = centerX, CenterY = centerY};
			RotateTransform viewPortRotation = new RotateTransform {Angle = Heading, CenterX = centerX, CenterY = centerY};
			TranslateTransform viewPortTranslation = new TranslateTransform {X = -dx, Y = -dy};

			ScaleTransform.ScaleX = scaleFactor;
			ScaleTransform.ScaleY = scaleFactor;
			RotateTransform.Angle = Heading;
			TranslationTransform.X = -dx;
			TranslationTransform.Y = -dy;

			TransformGroup transform = new TransformGroup();
			transform.Children.Add(viewPortScale);
			transform.Children.Add(viewPortRotation);
			transform.Children.Add(viewPortTranslation);
			ViewPortTransform = transform;
			InvalidateArrange();
			OnViewPortChangedEvent();
		}

		protected virtual void OnMapCenterChanged(Point newCenter)
		{
			EventHandler<Point> mapCenterChangedEvent = MapCenterChangedEvent;
			if (mapCenterChangedEvent != null)
			{
				mapCenterChangedEvent(this, newCenter);
			}
			_viewPortCenter = ViewPortProjection.ToCartesian(MapCenter);
			if (_viewPortCenter.X > 128 || _viewPortCenter.X < -128)
			{ }
			UpdateViewPortTransform();
		}
	}
}