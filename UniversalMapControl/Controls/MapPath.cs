using System.Collections.ObjectModel;
using System.Linq;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Projections;

namespace UniversalMapControl.Controls
{
	public class MapPath : BaseMapPath
	{
		private static void PathItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MapPath mapPath = (MapPath)d;
			mapPath.OnPathItemsChanged((ObservableCollection<ILocation>)e.NewValue);
		}

		public static readonly DependencyProperty PathItemsProperty = DependencyProperty.Register("PathItems", typeof(ObservableCollection<ILocation>), typeof(MapPath), new PropertyMetadata(new Wgs84Location(), PathItemsPropertyChanged));

		public ObservableCollection<ILocation> PathItems
		{
			get { return (ObservableCollection<ILocation>)GetValue(PathItemsProperty); }
			set { SetValue(PathItemsProperty, value); }
		}


		private void OnPathItemsChanged(ObservableCollection<ILocation> newLocations)
		{
			UpdatePath();
		}

		public virtual void UpdatePath()
		{
			if (ParentMap == null || PathItems.Count < 2)
			{
				Data = new PathGeometry();
				return;
			}

			PathGeometry pathGeometry = new PathGeometry();
			pathGeometry.Transform = ParentMap.ViewPortTransform;
			PathFigure pathFigure = new PathFigure();
			pathFigure.IsClosed = false;
			pathFigure.IsFilled = false;
			pathGeometry.Figures.Add(pathFigure);

			CartesianPoint startPoint = ParentMap.ViewPortProjection.ToCartesian(PathItems.First());
			pathFigure.StartPoint = new Point(startPoint.X, startPoint.Y);

			PolyLineSegment segment = new PolyLineSegment();
			foreach (ILocation point in PathItems.Skip(1))
			{
				CartesianPoint cartesianPoint = ParentMap.ViewPortProjection.ToCartesian(point);
				segment.Points.Add(new Point(cartesianPoint.X, cartesianPoint.Y));
			}
			pathFigure.Segments.Add(segment);

			Data = pathGeometry;
		}
	}
}