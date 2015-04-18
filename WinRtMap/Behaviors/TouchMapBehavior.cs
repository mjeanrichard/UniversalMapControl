using System;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace WinRtMap.Behaviors
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
		}

		public DependencyObject AssociatedObject { get; private set; }
		public bool TranslationEnabled { get; set; }
		public bool RotationEnabled { get; set; }
		public bool ZoomEnabled { get; set; }

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
		}

		public void Detach()
		{
			_map.ManipulationStarted -= OnManipulationStarted;
			_map.ManipulationCompleted -= OnManipulationCompleted;
			_map.ManipulationDelta -= OnManipulationDelta;
			_map = null;
			AssociatedObject = null;
		}
	}
}