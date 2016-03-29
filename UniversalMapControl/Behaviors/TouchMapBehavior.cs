using System;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace UniversalMapControl.Behaviors
{
	/// <summary>
	/// Adds basic (Translation, Rotation and Zoom) Touch Manipulation to a Map. 
	/// The Map must have set its ManipulationMode property accodingly.
	/// </summary>
	public class TouchMapBehavior : DependencyObject, IBehavior
	{
		private double _headingBeforeManipulation;
		private Point _manipulationStartPoint;
		private Map _map;
		private Point _viewPortCenterBeforeManipulation;
		private double _zoomBeforeManipulation;

		public TouchMapBehavior()
		{
			TranslationEnabled = true;
			RotationEnabled = true;
			ZoomEnabled = true;
			WheelEnabled = true;
			DoubleTapEnabled = true;

			ZoomWheelDelta = 0.001;
			DoubleTapDelta = 1;
		}

		public DependencyObject AssociatedObject { get; private set; }

		/// <summary>
		/// Determines where translations are activated. If set to false the User will not be able to move the map by moving it.
		/// </summary>
		public bool TranslationEnabled { get; set; }

		/// <summary>
		/// Determines if the User can rotate the Map with a rotate gesture.
		/// </summary>
		public bool RotationEnabled { get; set; }

		/// <summary>
		/// Determines whether the user can zoom the map with a zoom gesture.
		/// </summary>
		public bool ZoomEnabled { get; set; }

		/// <summary>
		/// Determines whether the User can Zoom the Map with the mouse wheel.
		/// Use ZoomWheelDelta to specify how much the zoomlevel is changed.
		/// </summary>
		public bool WheelEnabled { get; set; }

		/// <summary>
		/// Determines whether the User can zoom into the Map by double clicking (or tapping).
		/// Use DoubleTapDelta to specify how much the zoomlevel is changed.
		/// </summary>
		public bool DoubleTapEnabled { get; set; }

		/// <summary>
		/// If set to true the user can press Shift while double tapping to zoom out.
		/// This setting is only used if DoubleTapEnabled
		/// </summary>
		public bool ShiftInvertsDoubleTap { get; set; }

		public double DoubleTapDelta { get; set; }
		public double ZoomWheelDelta { get; set; }

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
			_headingBeforeManipulation = _map.Heading;
			_zoomBeforeManipulation = _map.ZoomLevel;
			_viewPortCenterBeforeManipulation = _map.ViewPortCenter;
			_manipulationStartPoint = _map.ViewPortTransform.Inverse.TransformPoint(e.Position);
			e.Handled = true;
		}

		protected virtual void UpdateManipulation(ManipulationDelta delta)
		{
			double newZoomLevel = _zoomBeforeManipulation + _map.ViewPortProjection.GetZoomLevel(delta.Scale);
			double newHeading = _headingBeforeManipulation;

			TransformGroup transform = new TransformGroup();

			double translationScaleFactor = 1 / _map.ViewPortProjection.GetZoomFactor(_zoomBeforeManipulation);

			if (TranslationEnabled)
			{
				TranslateTransform translate = new TranslateTransform {X = -delta.Translation.X * translationScaleFactor, Y = -delta.Translation.Y * translationScaleFactor};
				transform.Children.Add(translate);
			}

			//Revert current Rotation of the Map (this Rotation was centered around the original ViewPortCenter)
			RotateTransform mapRotation = new RotateTransform {Angle = -_headingBeforeManipulation, CenterX = _viewPortCenterBeforeManipulation.X, CenterY = _viewPortCenterBeforeManipulation.Y};
			transform.Children.Add(mapRotation);

			if (delta.Rotation != 0 && RotationEnabled)
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

			if (ZoomEnabled)
			{
				double scaleFactor = _map.ViewPortProjection.GetZoomFactor(_zoomBeforeManipulation - newZoomLevel);
				Transform scale = new ScaleTransform {ScaleX = scaleFactor, ScaleY = scaleFactor, CenterX = _manipulationStartPoint.X, CenterY = _manipulationStartPoint.Y};
				transform.Children.Add(scale);
			}

			_map.Heading = newHeading;
			_map.ZoomLevel = newZoomLevel;
			_map.ViewPortCenter = transform.TransformPoint(_viewPortCenterBeforeManipulation);
		}

		protected virtual void UpdateZoomOnlyManipulation(double zoomDelta, Point position)
		{
			Point zoomCenter = _map.ViewPortTransform.Inverse.TransformPoint(position);

			double scaleFactor = 1 / _map.ViewPortProjection.GetZoomFactor(zoomDelta);
			Transform scale = new ScaleTransform { ScaleX = scaleFactor, ScaleY = scaleFactor, CenterX = zoomCenter.X, CenterY = zoomCenter.Y };

			_map.ViewPortCenter = scale.TransformPoint(_map.ViewPortCenter);
			_map.ZoomLevel = _map.ZoomLevel + zoomDelta;
		}


		private void MapOnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
		{
			if (_map != null && WheelEnabled)
			{
				PointerPoint currentPoint = e.GetCurrentPoint(_map);
				double zoomDelta = currentPoint.Properties.MouseWheelDelta * ZoomWheelDelta;
				UpdateZoomOnlyManipulation(zoomDelta, currentPoint.RawPosition);
				e.Handled = true;
			}
		}

		private void MapOnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			if (_map != null && DoubleTapEnabled)
			{
				Point position = e.GetPosition(_map);
				double zoomDelta = DoubleTapDelta;
				if (ShiftInvertsDoubleTap && Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
				{
					zoomDelta = -zoomDelta;
				}
				UpdateZoomOnlyManipulation(zoomDelta, position);
				e.Handled = true;
			}
		}


		public void Attach(DependencyObject associatedObject)
		{
			if (!(associatedObject is Map))
			{
				throw new InvalidOperationException("The TouchMapBehavior can only be used for a Map.");
			}
			AssociatedObject = associatedObject;
			_map = (Map)associatedObject;
			_map.ManipulationStarted += OnManipulationStarted;
			_map.ManipulationCompleted += OnManipulationCompleted;
			_map.ManipulationDelta += OnManipulationDelta;
			_map.DoubleTapped += MapOnDoubleTapped;
			_map.PointerWheelChanged += MapOnPointerWheelChanged;
		}

		public void Detach()
		{
			_map.ManipulationStarted -= OnManipulationStarted;
			_map.ManipulationCompleted -= OnManipulationCompleted;
			_map.ManipulationDelta -= OnManipulationDelta;
			_map.DoubleTapped -= MapOnDoubleTapped;
			_map.PointerWheelChanged -= MapOnPointerWheelChanged;
			_map = null;
			AssociatedObject = null;
		}
	}
}