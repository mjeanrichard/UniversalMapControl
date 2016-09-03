using System;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

using Microsoft.Xaml.Interactivity;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Projections;

namespace UniversalMapControl.Behaviors
{
    public class AnimatedValuesBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty TargetZoomProperty = DependencyProperty.Register("TargetZoom", typeof(double), typeof(AnimatedValuesBehavior), new PropertyMetadata(0d, TargetZoomPropertyChanged));
        public static readonly DependencyProperty TargetHeadingProperty = DependencyProperty.Register("TargetHeading", typeof(double), typeof(AnimatedValuesBehavior), new PropertyMetadata(0d, TargetHeadingPropertyChanged));
        public static readonly DependencyProperty TargetCenterProperty = DependencyProperty.Register("TargetCenter", typeof(ILocation), typeof(AnimatedValuesBehavior), new PropertyMetadata(new Wgs84Location(), TargetCenterPropertyChanged));
        private bool _isBehaviorEnabled = false;

        private Map _map;

        private static void TargetZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedValuesBehavior)d).TargetZoomPropertyChanged(e);
        }

        private static void TargetHeadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedValuesBehavior)d).TargetHeadingPropertyChanged(e);
        }

        private static void TargetCenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AnimatedValuesBehavior)d).TargetCenterPropertyChanged(e);
        }

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

        public ILocation TargetCenter
        {
            get { return (ILocation)GetValue(TargetCenterProperty); }
            set { SetValue(TargetCenterProperty, value); }
        }

        public DependencyObject AssociatedObject { get; private set; }

        protected virtual void TargetZoomPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!_isBehaviorEnabled)
            {
                return;
            }

            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;

            int timeMs = (int)(Math.Abs(oldValue - newValue) * 200);

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

        protected virtual void TargetCenterPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!_isBehaviorEnabled)
            {
                return;
            }

            Point newValue = (Point)e.NewValue;

            PointAnimation centerAnimation = new PointAnimation
            {
                To = newValue,
                Duration = new Duration(TimeSpan.FromMilliseconds(1000)),
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut },
                FillBehavior = FillBehavior.HoldEnd,
                EnableDependentAnimation = true
            };
            Storyboard.SetTargetProperty(centerAnimation, "MapCenter");
            Storyboard.SetTarget(centerAnimation, _map);
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(centerAnimation);
            storyboard.Begin();
        }

        protected virtual void TargetHeadingPropertyChanged(DependencyPropertyChangedEventArgs e)
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
                throw new InvalidOperationException("The AnimatedValuesBehavior can only be used for a Map.");
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