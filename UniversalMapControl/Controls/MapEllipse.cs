using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Controls
{
	public class MapEllipse : BaseMapPath
	{
		public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register("RadiusX", typeof(double), typeof(MapEllipse), new PropertyMetadata(default(double), OnLayoutPropertyChanged));

		public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register("RadiusY", typeof(double), typeof(MapEllipse), new PropertyMetadata(default(double), OnLayoutPropertyChanged));

		public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(ILocation), typeof(MapEllipse), new PropertyMetadata(default(ILocation), OnLayoutPropertyChanged));

		private EllipseGeometry _ellipse;

		private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((MapEllipse)d).Update();
		}

		public double RadiusX
		{
			get { return (double)GetValue(RadiusXProperty); }
			set { SetValue(RadiusXProperty, value); }
		}

		public double RadiusY
		{
			get { return (double)GetValue(RadiusYProperty); }
			set { SetValue(RadiusYProperty, value); }
		}

		public ILocation Center
		{
			get { return (ILocation)GetValue(CenterProperty); }
			set { SetValue(CenterProperty, value); }
		}

		private void Update()
		{
			ILocation center = Center;

			if (ParentMap == null || center == null)
			{
				return;
			}
			IProjection viewPortProjection = ParentMap.ViewPortProjection;
			double scaleFactor = viewPortProjection.CartesianScaleFactor(center);
			_ellipse.RadiusY = RadiusY * scaleFactor;
			_ellipse.RadiusX = RadiusX * scaleFactor;
			_ellipse.Center = viewPortProjection.ToCartesian(center).ToPoint();
			InvalidateMeasure();
		}

		protected override void OnLoaded(object sender, RoutedEventArgs e)
		{
			base.OnLoaded(sender, e);
			ParentMap.ProjectionChanged += (s, args) => Update();

			_ellipse = new EllipseGeometry();
			_ellipse.Transform = ParentMap.ViewPortTransform;
			Update();
			Data = _ellipse;
		}
	}
}