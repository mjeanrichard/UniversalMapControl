using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Xaml.Interactivity;

namespace WinRtMap.Behaviors
{
	public class AnimatedValuesBehavior : DependencyObject, IBehavior
	{
		private bool _isBehaviorEnabled = false;

		public static readonly DependencyProperty TargetZoomProperty = DependencyProperty.Register("TargetZoom", typeof(double), typeof(Map), new PropertyMetadata(0d, TargetZoomPropertyChanged));
		public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register("TargetHeading", typeof(double), typeof(Map), new PropertyMetadata(0d, TargetHeadingPropertyChanged));

		private static void TargetZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((AnimatedValuesBehavior)d).TargetZoomPropertyChanged(e);
		}

		private static void TargetHeadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((AnimatedValuesBehavior)d).TargetHeadingPropertyChanged(e);
		}

		private Map _map;

		public double TargetZoom
		{
			get { return (double)GetValue(TargetZoomProperty); }
			set { SetValue(TargetZoomProperty, value); }
		}

		public double TargetHeading
		{
			get { return (double)GetValue(TargetHeadingProperty); }
			set { SetValue(TargetHeadingProperty, value); }
		}

		public DependencyObject AssociatedObject { get; private set; }

		private void TargetZoomPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (!_isBehaviorEnabled)
			{
				return;
			}

            double oldValue = (double)e.OldValue;
			double newValue = (double)e.NewValue;

			int timeMs = (int)(Math.Abs(oldValue - newValue)*200);

			DoubleAnimation zoomAnimation = new DoubleAnimation
			{
				To = newValue,
				Duration = new Duration(TimeSpan.FromMilliseconds(timeMs)),
				EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut },
				FillBehavior = FillBehavior.HoldEnd,
				EnableDependentAnimation = true
			};
			Storyboard.SetTargetProperty(zoomAnimation, "ZoomLevel");
			Storyboard.SetTarget(zoomAnimation, _map);
			Storyboard storyboard = new Storyboard();
			storyboard.Children.Add(zoomAnimation);
			storyboard.Begin();
		}

		private void TargetHeadingPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (!_isBehaviorEnabled)
			{
				return;
			}

			double newValue = (double)e.NewValue;

			DoubleAnimation zoomAnimation = new DoubleAnimation
			{
				To = newValue,
				Duration = new Duration(TimeSpan.FromMilliseconds(500)),
				EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut },
				FillBehavior = FillBehavior.HoldEnd,
				EnableDependentAnimation = true
			};
			Storyboard.SetTargetProperty(zoomAnimation, "Heading");
			Storyboard.SetTarget(zoomAnimation, _map);
			Storyboard storyboard = new Storyboard();
			storyboard.Children.Add(zoomAnimation);
			storyboard.Begin();
		}

		public void Attach(DependencyObject associatedObject)
		{
			_isBehaviorEnabled = false;
			if (!(associatedObject is Map))
			{
				throw new InvalidOperationException("The TouchMapBehavior can only be used for a Map.");
			}
			AssociatedObject = associatedObject;
			_map = (Map)associatedObject;

			TargetZoom = _map.ZoomLevel;
			TargetHeading = _map.Heading;

			_isBehaviorEnabled = true;
		}

		public void Detach()
		{
			_isBehaviorEnabled = false;
			_map = null;
			AssociatedObject = null;
		}
	}
}