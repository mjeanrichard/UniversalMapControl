using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Controls
{
    public class MapEllipse : BaseMapPath
    {
        public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register("RadiusX", typeof(double), typeof(MapEllipse), new PropertyMetadata(default(double), OnLayoutPropertyChanged));

        public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register("RadiusY", typeof(double), typeof(MapEllipse), new PropertyMetadata(default(double), OnLayoutPropertyChanged));

        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(string), typeof(MapEllipse), new PropertyMetadata(default(string), OnLayoutPropertyChanged));

        private EllipseGeometry _ellipse;

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MapEllipse)d).Invalidate();
        }

        /// <summary>
        /// X Radius of the Ellipse in Meters
        /// </summary>
        public double RadiusX
        {
            get { return (double)GetValue(RadiusXProperty); }
            set { SetValue(RadiusXProperty, value); }
        }

        /// <summary>
        /// Y Radius of the Ellipse in Meters
        /// </summary>
        public double RadiusY
        {
            get { return (double)GetValue(RadiusYProperty); }
            set { SetValue(RadiusYProperty, value); }
        }

        public string Center
        {
            get { return (string)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        protected override void Invalidate()
        {
            string stringCenter = Center;
            if (ParentMap == null || string.IsNullOrWhiteSpace(stringCenter))
            {
                return;
            }
            if (_ellipse == null)
            {
                _ellipse = new EllipseGeometry();
                _ellipse.Transform = ParentMap.ViewPortTransform;
                Data = _ellipse;
            }

            IProjection viewPortProjection = ParentMap.ViewPortProjection;
            ILocation center = viewPortProjection.ParseLocation(stringCenter);

            double scaleFactor = viewPortProjection.CartesianScaleFactor(center);
            _ellipse.RadiusY = RadiusY * scaleFactor;
            _ellipse.RadiusX = RadiusX * scaleFactor;
            _ellipse.Center = viewPortProjection.ToCartesian(center).ToPoint();
            InvalidateMeasure();
        }
    }
}