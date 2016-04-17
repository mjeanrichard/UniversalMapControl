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
			typeof(Location),
			typeof(MapLayerBase),
			new PropertyMetadata(new Location(double.NaN, double.NaN), OnLocationPropertyChange));

	    public static readonly DependencyProperty LatitudeProperty = DependencyProperty.RegisterAttached("Latitude",
			typeof(double),
			typeof(MapLayerBase),
			new PropertyMetadata(double.NaN));

		public static readonly DependencyProperty LongitudeProperty = DependencyProperty.RegisterAttached("Longitude",
			typeof(double),
			typeof(MapLayerBase),
			new PropertyMetadata(double.NaN));

		public static double GetLatitude(DependencyObject child)
		{
			Location location = (Location)child.GetValue(LocationProperty);
			return location.Latitude;
		}

		public static void SetLatitude(DependencyObject child, double value)
		{
			Location location = GetLocation(child);
			location.Latitude = value;
			SetLocation(child, location);
		}

		public static double GetLongitude(DependencyObject child)
		{
			Location location = (Location)child.GetValue(LocationProperty);
			return location.Longitude;
		}

		public static void SetLongitude(DependencyObject child, double value)
		{
			Location location = GetLocation(child);
			location.Longitude = value;
			SetLocation(child, location);
		}

		public static Location GetLocation(DependencyObject child)
		{
			return (Location)child.GetValue(LocationProperty);
		}

		public static void SetLocation(DependencyObject child, Location value)
		{
			child.SetValue(LocationProperty, value);
		}

		private static void OnLocationPropertyChange(DependencyObject child, DependencyPropertyChangedEventArgs e)
		{
			MapLayerBase mapLayer = VisualTreeHelper.GetParent(child) as MapLayerBase;
			if (mapLayer != null)
			{
				mapLayer.InvalidateArrange();
			}
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

		protected virtual Location? GetLocationPropertyValueIfSet(DependencyObject child)
		{
			Location location = GetLocation(child);
			if (double.IsNaN(location.Latitude) || double.IsNaN(location.Longitude))
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
			if (element is CanvasMapLayer)
			{
				element.Arrange(new Rect(new Point(0, 0), finalSize));
				((CanvasMapLayer)element).Invalidate();
				return;
			}

			Location? location = GetLocation(element);

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

	    protected virtual Location? GetLocation(UIElement element)
	    {
	        IHasLocation elementWithLocation = element as IHasLocation;
			Location? location;
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

		protected virtual Point GetPositionForElementWithLocation(Location location, UIElement element, Map parentMap)
		{
			Size desiredSize = element.DesiredSize;
			Point position = parentMap.ViewPortProjection.ToCartesian(location, parentMap.MapCenter.Longitude);

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