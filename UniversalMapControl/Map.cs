using System;
using System.Runtime.InteropServices;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

using Microsoft.Xaml.Interactions.Core;

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
            ManipulationMode = ManipulationModes.Rotate | ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;

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
        public event EventHandler ProjectionChanged;

        public IProjection ViewPortProjection
        {
            get { return _viewPortProjection; }
            set
            {
                _viewPortProjection = value;
                OnProjectionChanged();
                if (_viewPortProjection != null)
                {
                    OnMapCenterChanged(MapCenter);
                }
            }
        }

        protected virtual void OnProjectionChanged()
        {
            ProjectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public MatrixTransform ViewPortTransform { get; set; }

        public ILocation MapCenter
        {
            get
            {
                ILocation mapCenter = (ILocation)GetValue(MapCenterProperty);
                if (mapCenter == null)
                {
                    return new Wgs84Location();
                }
                return mapCenter;
            }
            set { SetValue(MapCenterProperty, value); }
        }

        public double Heading
        {
            get { return (double)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        private double _zoomLevel;

        public double ZoomLevel
        {
            get { return _zoomLevel; }
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
            _zoomLevel = newZoomLevel;
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

            double w2 = ActualWidth / 2d;
            double h2 = ActualHeight / 2d;

            MatrixDouble vpCenterTranslation = MatrixDouble.CreateTranslation(w2, h2);
            MatrixDouble scale = MatrixDouble.CreateScale(scaleFactor);

            MatrixDouble mapCenterTranslation = MatrixDouble.CreateTranslation(-ViewPortCenter.X, -ViewPortCenter.Y);

            double heading = Heading * Math.PI / 180.0;
            Point center = new Point(ViewPortCenter.X, ViewPortCenter.Y);
            MatrixDouble mapRotation = MatrixDouble.CreateRotation(heading, center);
            MatrixDouble objectRotation = MatrixDouble.CreateRotation(heading);

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
        /// This Method can be use to convert a Point on the Map to a Location (in the current Projection).
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

            double x = (point.X - RenderSize.Width / 2d) * zoomFactor + ViewPortCenter.X;
            double y = (point.Y - RenderSize.Height / 2d) * zoomFactor + ViewPortCenter.Y;

            MatrixDouble reverseRotationMatrix = MatrixDouble.CreateRotation(-TransformHelper.DegToRad(Heading), ViewPortCenter.ToPoint());

            return new CartesianPoint(reverseRotationMatrix.Transform(new Point(x, y)));
        }

        /// <summary>
        /// This function calculates the smallest axis-aligned bounding box possible for the current ViewPort. 
        /// This means that if the current Map has a Heading that is not a multiple of 90° 
        /// this function will return a bounding box that is bigger than the actual ViewPort.
        /// </summary>
        public virtual Rect GetViewportBounds()
        {
            double zoomFactor = ViewPortProjection.GetZoomFactor(ZoomLevel);
            double halfHeight = RenderSize.Height / (2 * zoomFactor);
            double halfWidth = RenderSize.Width / (2 * zoomFactor);

            Point topLeft = new Point(ViewPortCenter.X - halfWidth, ViewPortCenter.Y - halfHeight);
            Point bottomRight = new Point(ViewPortCenter.X + halfWidth, ViewPortCenter.Y + halfHeight);

            RotateTransform rotation = new RotateTransform { Angle = Heading, CenterX = ViewPortCenter.X, CenterY = ViewPortCenter.Y };

            Rect rect = new Rect(topLeft, bottomRight);
            rect = rotation.TransformBounds(rect);
            return rect;
        }

        public virtual void ZoomToRect(ILocation l1, ILocation l2, double zoomCorrectionFactor = 0.9d)
        {
            CartesianPoint c1 = ViewPortProjection.ToCartesian(l1);
            CartesianPoint c2 = ViewPortProjection.ToCartesian(l2);

            Rect bounds = new Rect(c1.ToPoint(), c2.ToPoint());
            RotateTransform rotation = new RotateTransform { Angle = Heading, CenterX = bounds.X / 2d, CenterY = bounds.Y / 2d };
            Rect rotatedBounds = rotation.TransformBounds(bounds);

            double fullZoomX = RenderSize.Width / rotatedBounds.Width;
            double fullZoomY = RenderSize.Height / rotatedBounds.Height;

            double zoomLevel = ViewPortProjection.GetZoomLevel(Math.Min(fullZoomX, fullZoomY) * zoomCorrectionFactor);

            ViewPortCenter = new CartesianPoint((long)Math.Round(bounds.X + bounds.Width / 2d), (long)Math.Round(bounds.Y + bounds.Height / 2d));
            ZoomLevel = zoomLevel;
        }
    }
}