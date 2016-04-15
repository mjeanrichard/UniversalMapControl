using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using UniversalMapControl.Utils;

namespace UniversalMapControl
{
	public class MapLayerBase : Panel
	{
	    public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached("Location",
			typeof(Point),
			typeof(MapLayerBase),
			new PropertyMetadata(new Point(double.NaN, double.NaN), OnLocationPropertyChange));

		public static Point GetLocation(DependencyObject child)
		{
			return (Point)child.GetValue(LocationProperty);
		}

		private static void OnLocationPropertyChange(DependencyObject child, DependencyPropertyChangedEventArgs e)
		{
			MapLayerBase mapLayer = VisualTreeHelper.GetParent(child) as MapLayerBase;
			if (mapLayer != null)
			{
				mapLayer.InvalidateArrange();
			}
		}

		public static void SetLocation(DependencyObject child, Point value)
		{
			child.SetValue(LocationProperty, value);
		}

		private Lazy<Map> _parentMap;

		protected MapLayerBase()
		{
			_parentMap = new Lazy<Map>(LoadParentMap);
		}

		protected Map ParentMap
		{
			get { return _parentMap.Value; }
		}

		protected virtual Point? GetLocationPropertyValueIfSet(DependencyObject child)
		{
		    Point location = GetLocation(child);
			if (double.IsNaN(location.X) || double.IsNaN(location.Y))
			{
				return null;
			}
			return location;
		}

		protected virtual Map LoadParentMap()
		{
			Map map = this.GetAncestor<Map>();
			if (map == null)
			{
				throw new InvalidOperationException("A MapLayer must have an ancestor of type Map.");
			}
			return map;
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			foreach (UIElement element in Children)
			{
				element.Measure(availableSize);
			}

			Size result = availableSize;
			if (availableSize.Height == Double.PositiveInfinity)
			{
				result.Height = 500;
			}
			if (availableSize.Width == Double.PositiveInfinity)
			{
				result.Height = 500;
			}
			return result;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Clip = new RectangleGeometry {Rect = new Rect(0, 0, finalSize.Width, finalSize.Height)};

			foreach (UIElement element in Children)
			{
				ArrangeElement(element, finalSize);
			}

			return base.ArrangeOverride(finalSize);
		}

		public void ArrangeElement(UIElement element, Size finalSize)
		{
			if (element is MapLayerBase)
			{
				element.Arrange(new Rect(new Point(0, 0), finalSize));
				return;
			}

			Point? location = GetLocation(element);

		    Map parentMap = ParentMap;
			Point finalPosition;
			if (location.HasValue)
			{
				finalPosition = GetPositionForElementWithLocation(location.Value, element, parentMap);
			}
			else
			{
				finalPosition = GetPositionForElementWithoutLocation(element, finalSize);
			}

			element.Arrange(new Rect(finalPosition, element.DesiredSize));
		}

	    protected virtual Point? GetLocation(UIElement element)
	    {
	        IHasLocation elementWithLocation = element as IHasLocation;
	        Point? location;
	        if (elementWithLocation != null)
	        {
	            location = elementWithLocation.Location;
	        }
	        else
	        {
	            location = GetLocationPropertyValueIfSet(element);
	        }
	        return location;
	    }

	    protected virtual Point GetPositionForElementWithoutLocation(UIElement element, Size finalPanelSize)
		{
			FrameworkElement frameworkElement = element as FrameworkElement;
			if (frameworkElement == null)
			{
				return new Point();
			}

			Size desiredSize = frameworkElement.DesiredSize;
			Point position;
			switch (frameworkElement.HorizontalAlignment)
			{
				case HorizontalAlignment.Right:
					position.X = finalPanelSize.Width - desiredSize.Width;
					break;
				case HorizontalAlignment.Center:
					position.X = (finalPanelSize.Width - desiredSize.Width) / 2;
					break;
				case HorizontalAlignment.Left:
				case HorizontalAlignment.Stretch:
				default:
					break;
			}
			switch (frameworkElement.VerticalAlignment)
			{
				case VerticalAlignment.Bottom:
					position.Y = finalPanelSize.Height - desiredSize.Height;
					break;
				case VerticalAlignment.Center:
					position.Y = (finalPanelSize.Height - desiredSize.Height) / 2;
					break;
				case VerticalAlignment.Top:
				case VerticalAlignment.Stretch:
				default:
					break;
			}
			return position;
		}

		protected virtual Point GetPositionForElementWithLocation(Point location, UIElement element, Map parentMap)
		{
			Size desiredSize = element.DesiredSize;
			Point position = parentMap.ViewPortProjection.ToCartesian(new Point(location.X, location.Y), parentMap.MapCenter.X);

			position = parentMap.ViewPortTransform.TransformPoint(position);

			FrameworkElement frameworkElement = element as FrameworkElement;
			if (frameworkElement != null)
			{
				switch (frameworkElement.HorizontalAlignment)
				{
					case HorizontalAlignment.Center:
						position.X -= desiredSize.Width / 2d;
						break;

					case HorizontalAlignment.Right:
						position.X -= desiredSize.Width;
						break;
				}

				switch (frameworkElement.VerticalAlignment)
				{
					case VerticalAlignment.Center:
						position.Y -= desiredSize.Height / 2d;
						break;

					case VerticalAlignment.Bottom:
						position.Y -= desiredSize.Height;
						break;
				}
			}


			return position;
		}
	}
}