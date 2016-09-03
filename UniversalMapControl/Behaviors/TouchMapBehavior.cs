using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

using Microsoft.Xaml.Interactivity;

using UniversalMapControl.Utils;

namespace UniversalMapControl.Behaviors
{
    /// <summary>
    /// Adds basic (Translation, Rotation and Zoom) Touch Manipulation to a Map. 
    /// The Map must have set its ManipulationMode property accodingly.
    /// </summary>
    public class TouchMapBehavior : DependencyObject, IBehavior, INotifyPropertyChanged
    {
        private Map _map;
        private double _headingBeforeManipulation;
        private Point _manipulationStartPoint;
        private Point _viewPortCenterBeforeManipulation;
        private double _zoomFactorBeforeManipulation;
        private MatrixDouble _reverseRotationMatrix;

        public TouchMapBehavior()
        {
            TranslationEnabled = true;
            RotationEnabled = true;
            ZoomEnabled = true;
            WheelEnabled = true;
            DoubleTapEnabled = true;
            AutoUpdateMap = true;

            ZoomWheelDelta = 0.001;
            DoubleTapDelta = 1;
        }

        public DependencyObject AssociatedObject { get; private set; }

        public event EventHandler<TouchMapEventArgs> Update;

        /// <summary>
        /// Determines if the associated map will be updated automatically.
        /// </summary>
        public bool AutoUpdateMap { get; set; }

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
            _zoomFactorBeforeManipulation = _map.ViewPortProjection.GetZoomFactor(_map.ZoomLevel);
            _viewPortCenterBeforeManipulation = _map.ViewPortCenter.ToPoint();

            _reverseRotationMatrix = MatrixDouble.CreateRotation(-TransformHelper.DegToRad(_headingBeforeManipulation), _viewPortCenterBeforeManipulation);

            _manipulationStartPoint = _map.GetCartesianFromPoint(e.Position).ToPoint();

            e.Handled = true;
        }

        protected virtual void UpdateManipulation(ManipulationDelta delta)
        {
            double newHeading = _headingBeforeManipulation;
            double newZoomFact = _zoomFactorBeforeManipulation * delta.Scale;

            MatrixDouble m = MatrixDouble.Identity;

            if (TranslationEnabled)
            {
                m = MatrixDouble.CreateTranslation(-(delta.Translation.X / _zoomFactorBeforeManipulation), -(delta.Translation.Y / _zoomFactorBeforeManipulation));
                m = m * _reverseRotationMatrix;
            }

            if (ZoomEnabled)
            {
                double scaleFactor = _zoomFactorBeforeManipulation / newZoomFact;
                m = m * MatrixDouble.CreateScale(scaleFactor, _manipulationStartPoint);
            }

            if ((delta.Rotation != 0.0) && RotationEnabled)
            {
                //Add the Rotation from the Manipulation
                MatrixDouble rotation = MatrixDouble.CreateRotation(-TransformHelper.DegToRad(delta.Rotation), _manipulationStartPoint);
                m = m * rotation;

                newHeading = (_headingBeforeManipulation + delta.Rotation) % 360;
                if (newHeading < 0)
                {
                    newHeading += 360;
                }
            }

            double zoomLevel = _map.ViewPortProjection.GetZoomLevel(newZoomFact);
            CartesianPoint viewPortCenter = new CartesianPoint(m.Transform(_viewPortCenterBeforeManipulation));
            if (Update != null)
            {
                TouchMapEventArgs eventArgs = new TouchMapEventArgs();
                eventArgs.Heading = newHeading;
                eventArgs.ZoomLevel = zoomLevel;
                eventArgs.ViewPortCenter = viewPortCenter;
                OnUpdate(eventArgs);
            }
            if (AutoUpdateMap)
            {
                _map.Heading = newHeading;
                _map.ZoomLevel = zoomLevel;
                _map.ViewPortCenter = viewPortCenter;
            }
        }

        protected virtual void UpdateZoomOnlyManipulation(double zoomDelta, Point position)
        {
            Point zoomCenter = _map.GetCartesianFromPoint(position).ToPoint();

            double oldZoomLevel = _map.ZoomLevel;
            double newZoomLevel = oldZoomLevel + zoomDelta;
            double scaleFactor = 1 / (_map.ViewPortProjection.GetZoomFactor(newZoomLevel) / _map.ViewPortProjection.GetZoomFactor(oldZoomLevel));

            MatrixDouble scale = MatrixDouble.CreateScale(scaleFactor, zoomCenter);

            _map.ViewPortCenter = new CartesianPoint(scale.Transform(_map.ViewPortCenter.ToPoint()));
            _map.ZoomLevel = newZoomLevel;
        }


        private void MapOnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if ((_map != null) && WheelEnabled)
            {
                PointerPoint currentPoint = e.GetCurrentPoint(_map);
                double zoomDelta = currentPoint.Properties.MouseWheelDelta * ZoomWheelDelta;
                UpdateZoomOnlyManipulation(zoomDelta, currentPoint.RawPosition);
                e.Handled = true;
            }
        }

        private void MapOnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if ((_map != null) && DoubleTapEnabled)
            {
                Point position = e.GetPosition(_map);
                double zoomDelta = DoubleTapDelta;
                if (ShiftInvertsDoubleTap && (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down))
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnUpdate(TouchMapEventArgs eventArgs)
        {
            Update?.Invoke(this, eventArgs);
        }
    }
}