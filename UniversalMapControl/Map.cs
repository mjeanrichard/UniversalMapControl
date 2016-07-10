using System;
using System.Numerics;
using System.Runtime.InteropServices;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Projections;
using UniversalMapControl.Utils;

namespace UniversalMapControl
{
	public class Map : MapLayerBase
	{
		public static readonly DependencyProperty MapCenterProperty = DependencyProperty.Register(
			"MapCenter", typeof(ILocation), typeof(Map), new PropertyMetadata(new Wgs84Location(), MapCenterPropertyChanged));

		public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
			"Heading", typeof(double), typeof(Map), new PropertyMetadata(0d, HeadingPropertyChanged));

		public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
			"ZoomLevel", typeof(double), typeof(Map), new PropertyMetadata(0d, ZoomLevelPropertyChanged));

		private CartesianPoint _viewPortCenter;
		private IProjection _viewPortProjection;

		public Map()
		{
			_viewPortProjection = new Wgs84WebMercatorProjection();
			ViewPortTransform = new MatrixTransform();

			ScaleTransform = new MatrixTransform();
			RotateTransform = new MatrixTransform();
			TranslationTransform = new MatrixTransform();
			ScaleRotateTransform = new MatrixTransform();

			MinZoomLevel = 0;
			MaxZoomLevel = 25;

			ZoomLevel = 1;
			SizeChanged += Map_SizeChanged;

			Background = new SolidColorBrush(Colors.Transparent);
			ManipulationMode = ManipulationModes.All;

			MapCenter = new Wgs84Location(0, 0);
		}

		private static void HeadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapHeadingChanged((double)e.NewValue);
		}

		private static void MapCenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnMapCenterChanged((ILocation)e.NewValue);
		}

		private static void ZoomLevelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Map map = (Map)d;
			map.OnZoomLevelChanged((double)e.NewValue);
		}

		public event EventHandler<ILocation> MapCenterChangedEvent;
		public event EventHandler<double> MapHeadingChangedEvent;
		public event EventHandler ViewPortChangedEvent;
		public event EventHandler<double> ZoomLevelChangedEvent;

		public IProjection ViewPortProjection
		{
			get { return _viewPortProjection; }
			set
			{
				_viewPortProjection = value;
				if (_viewPortProjection != null)
				{
					OnMapCenterChanged(MapCenter);
				}
			}
		}

		public MatrixTransform ViewPortTransform { get; set; }

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

		public MatrixTransform ScaleTransform { get; }
		public MatrixTransform RotateTransform { get; }
		public MatrixTransform ScaleRotateTransform { get; }
		public MatrixTransform TranslationTransform { get; set; }

		public CartesianPoint ViewPortCenter
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
			if (ViewPortTransform == null)
			{
				return;
			}

			double scaleFactor = ViewPortProjection.GetZoomFactor(ZoomLevel);

			float w2 = (float)(ActualWidth / 2f);
			float h2 = (float)(ActualHeight / 2f);

			Matrix3x2 vpCenterTranslation = Matrix3x2.CreateTranslation(w2, h2);
			Matrix3x2 scale = Matrix3x2.CreateScale((float)scaleFactor);

			Matrix3x2 mapCenterTranslation = Matrix3x2.CreateTranslation(-(float)ViewPortCenter.X, -(float)ViewPortCenter.Y);

			double heading = Heading * Math.PI / 180.0;
			Vector2 center = new Vector2((float)ViewPortCenter.X, (float)ViewPortCenter.Y);
			Matrix3x2 mapRotation = Matrix3x2.CreateRotation((float)heading, center);
			Matrix3x2 objectRotation = Matrix3x2.CreateRotation((float)heading);

			ViewPortTransform.Matrix = (mapRotation * mapCenterTranslation * scale * vpCenterTranslation).ToXamlMatrix();
			ScaleRotateTransform.Matrix = (objectRotation * scale).ToXamlMatrix();
			ScaleTransform.Matrix = scale.ToXamlMatrix();
			RotateTransform.Matrix = objectRotation.ToXamlMatrix();
			TranslationTransform.Matrix = (mapCenterTranslation * vpCenterTranslation).ToXamlMatrix();

			InvalidateArrange();
			OnViewPortChangedEvent();
		}

		protected virtual void OnMapCenterChanged(ILocation newCenter)
		{
			MapCenterChangedEvent?.Invoke(this, newCenter);
			if (ViewPortProjection != null)
			{
				_viewPortCenter = ViewPortProjection.ToCartesian(MapCenter);
				UpdateViewPortTransform();
			}
		}


		/// <summary>
		/// This Method can be use to convert a Point on the Map to a Location (in the current Porjection).
		/// </summary>
		/// <param name="point">A Position on the MapControl (such as the MousePointer)</param>
		/// <returns>The location in the current Projection.</returns>
		public ILocation GetLocationFromPoint(Point point)
		{
			try
			{
				Point cartesianLocation = ViewPortTransform.Inverse.TransformPoint(point);
				ILocation position = ViewPortProjection.ToLocation(new CartesianPoint(cartesianLocation));
				return position;
			}
			catch (COMException)
			{
				//Inverse Matrix does not exist...
				return new Wgs84Location();
			}
		}

		public CartesianPoint GetCartesianFromPoint(Point point)
		{
			double zoomFactor = 1 / ViewPortProjection.GetZoomFactor(ZoomLevel);
			Vector2 delta = (point.ToVector2() - RenderSize.ToVector2() / 2) * (float)zoomFactor;

			Matrix3x2 reverseRotationMatrix = Matrix3x2.CreateRotation(-TransformHelper.DegToRad(Heading), ViewPortCenter.ToVector());

			return new CartesianPoint(Vector2.Transform(ViewPortCenter.ToVector() + delta, reverseRotationMatrix));

		}

		/// <summary>
		/// This function calculates the smallest axis-aligned bounding box possible for the current ViewPort. 
		/// This means that if the current Map has a Heading that is not a multiple of 90° 
		/// this function will a bounding box that is bigger than the actual ViewPort.
		/// </summary>
		public virtual Rect GetViewportBounds()
		{
			double zoomFactor = ViewPortProjection.GetZoomFactor(ZoomLevel);
			double halfHeight = RenderSize.Height / (2 * zoomFactor);
			double halfWidth = RenderSize.Width / (2 * zoomFactor);

			Point topLeft = new Point(ViewPortCenter.X - halfWidth, ViewPortCenter.Y - halfHeight);
			Point bottomRight = new Point(ViewPortCenter.X + halfWidth, ViewPortCenter.Y + halfHeight);

			RotateTransform rotation = new RotateTransform { Angle = Heading, CenterY = ViewPortCenter.Y, CenterX = ViewPortCenter.X };

			Rect rect = new Rect(topLeft, bottomRight);
			rect = rotation.TransformBounds(rect);
			return rect;
		}
	}
}