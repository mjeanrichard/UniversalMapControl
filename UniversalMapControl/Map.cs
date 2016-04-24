using System;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Projections;

namespace UniversalMapControl
{
	public class Map : MapLayerBase
	{
		public static readonly DependencyProperty MapCenterProperty = DependencyProperty.Register(
			"MapCenter", typeof(Location), typeof(Map), new PropertyMetadata(new Location(), MapCenterPropertyChanged));

		public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
			"Heading", typeof(double), typeof(Map), new PropertyMetadata(0d, HeadingPropertyChanged));

		public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
			"ZoomLevel", typeof(double), typeof(Map), new PropertyMetadata(0d, ZoomLevelPropertyChanged));

		private Point _viewPortCenter;

		public Map()
		{
			ViewPortProjection = new Wgs84WebMercatorProjection();
			ViewPortTransform = new TranslateTransform();

			ScaleTransform = new ScaleTransform();
			RotateTransform = new RotateTransform();
			TranslationTransform = new TranslateTransform();
			ScaleRotateTransform = new TransformGroup { Children = { ScaleTransform, RotateTransform } };

			MinZoomLevel = 0;
			MaxZoomLevel = 19;

			ZoomLevel = 1;
			SizeChanged += Map_SizeChanged;

			Background = new SolidColorBrush(Colors.Transparent);
			ManipulationMode = ManipulationModes.All;

			MapCenter = new Location(0, 0);
		}

		private static void HeadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapHeadingChanged((double)e.NewValue);
		}

		private static void MapCenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapCenterChanged((Location)e.NewValue);
		}

		private static void ZoomLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnZoomLevelChanged((double)e.NewValue);
		}

		public event EventHandler<Location> MapCenterChangedEvent;
		public event EventHandler<double> MapHeadingChangedEvent;
		public event EventHandler ViewPortChangedEvent;
		public event EventHandler<double> ZoomLevelChangedEvent;
		public IProjection ViewPortProjection { get; set; }

		public ILocation MapCenter
		{
			get { return (ILocation)GetValue(MapCenterProperty); }
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
					MapCenter = ViewPortProjection.ToLocation(value);
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
			MapHeadingChangedEvent?.Invoke(this, newHeading);
			UpdateViewPortTransform();
		}

		protected virtual void OnViewPortChangedEvent()
		{
			ViewPortChangedEvent?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnZoomLevelChanged(double newZoomLevel)
		{
			ZoomLevelChangedEvent?.Invoke(this, newZoomLevel);
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
			double dx = centerX - ActualWidth / 2;
			double dy = centerY - ActualHeight / 2;

			ScaleTransform viewPortScale = new ScaleTransform { ScaleY = scaleFactor, ScaleX = scaleFactor, CenterX = centerX, CenterY = centerY };
			RotateTransform viewPortRotation = new RotateTransform { Angle = Heading, CenterX = centerX, CenterY = centerY };
			TranslateTransform viewPortTranslation = new TranslateTransform { X = -dx, Y = -dy };

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

		protected virtual void OnMapCenterChanged(Location newCenter)
		{
			MapCenterChangedEvent?.Invoke(this, newCenter);
			_viewPortCenter = ViewPortProjection.ToCartesian(MapCenter);
			UpdateViewPortTransform();
		}


		/// <summary>
		/// This Method can be use to convert a Point on the Map to a Location (in the current Porjection).
		/// </summary>
		/// <param name="point">A Position on the MapControl (such as the MousePointer)</param>
		/// <returns>The location in the current Projection.</returns>
		public ILocation GetLocationFromPoint(Point point)
		{
			Point cartesianLocation = ViewPortTransform.Inverse.TransformPoint(point);
			ILocation position = ViewPortProjection.ToLocation(cartesianLocation);
			return position;
		}
	}
}