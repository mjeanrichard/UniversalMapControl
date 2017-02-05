using System.Collections.ObjectModel;
using System.Linq;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Controls
{
    public class MapPath : BaseMapPath
    {
        public static readonly DependencyProperty PathItemsProperty = DependencyProperty.Register("PathItems", typeof(ObservableCollection<string>), typeof(MapPath), new PropertyMetadata(new ObservableCollection<string>(), PathItemsPropertyChanged));

        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.Register("IsClosed", typeof(bool), typeof(MapPath), new PropertyMetadata(false, PathPropertiesChanged));

        public static readonly DependencyProperty IsFilledProperty = DependencyProperty.Register("IsFilled", typeof(bool), typeof(MapPath), new PropertyMetadata(false, PathPropertiesChanged));

        private static void PathItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapPath mapPath = (MapPath)d;
            mapPath.OnPathItemsChanged((ObservableCollection<ILocation>)e.NewValue);
        }

        private static void PathPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapPath mapPath = (MapPath)d;
            mapPath.Invalidate();
        }

        public bool IsClosed
        {
            get { return (bool)GetValue(IsClosedProperty); }
            set { SetValue(IsClosedProperty, value); }
        }

        public bool IsFilled
        {
            get { return (bool)GetValue(IsFilledProperty); }
            set { SetValue(IsFilledProperty, value); }
        }

        public ObservableCollection<string> PathItems
        {
            get { return (ObservableCollection<string>)GetValue(PathItemsProperty); }
            set { SetValue(PathItemsProperty, value); }
        }


        private void OnPathItemsChanged(ObservableCollection<ILocation> newLocations)
        {
            Invalidate();
        }

        protected override void Invalidate()
        {
            if (ParentMap == null || PathItems.Count < 2)
            {
                Data = new PathGeometry();
                return;
            }

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Transform = ParentMap.ViewPortTransform;
            PathFigure pathFigure = new PathFigure();
            pathFigure.IsClosed = IsClosed;
            pathFigure.IsFilled = IsFilled;
            pathGeometry.Figures.Add(pathFigure);

            ILocation location = ParentMap.ViewPortProjection.ParseLocation(PathItems.First());
            CartesianPoint startPoint = ParentMap.ViewPortProjection.ToCartesian(location);
            pathFigure.StartPoint = new Point(startPoint.X, startPoint.Y);

            PolyLineSegment segment = new PolyLineSegment();
            foreach (string stringLocation in PathItems.Skip(1))
            {
                location = ParentMap.ViewPortProjection.ParseLocation(stringLocation);
                CartesianPoint cartesianPoint = ParentMap.ViewPortProjection.ToCartesian(location);
                segment.Points.Add(new Point(cartesianPoint.X, cartesianPoint.Y));
            }
            pathFigure.Segments.Add(segment);

            Data = pathGeometry;
        }
    }
}