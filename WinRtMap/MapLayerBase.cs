using System;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WinRtMap.Tiles;
using WinRtMap.Utils;

namespace WinRtMap
{
	public class MapLayerBase : Panel
	{
		protected MapLayerBase()
		{
			Loaded += OnLoaded;
		}

		public static void SetLocation(DependencyObject child, Point value)
		{
			child.SetValue(LocationProperty, value);
		}

		public static Point GetLocation(DependencyObject child)
		{
			return (Point)child.GetValue(LocationProperty);
		}

		public static void SetRotateWithMap(DependencyObject child, bool value)
		{
			child.SetValue(RotateWithMapProperty, value);
		}

		public static bool GetRotateWithMap(DependencyObject child)
		{
			return (bool)child.GetValue(RotateWithMapProperty);
		}

		protected virtual Point? GetLocationIfSet(DependencyObject child)
		{
			object value = child.ReadLocalValue(LocationProperty);
			if (value == DependencyProperty.UnsetValue)
			{
				return null;
			}
			return (Point)value;
		}

		protected virtual Map GetParentMap()
		{
			Map parent = VisualTreeHelper.GetParent(this) as Map;
			if (parent == null)
			{
				//Ups?
			}
			return parent;
		}

		private static void OnLocationPropertyChange(DependencyObject child, DependencyPropertyChangedEventArgs e)
		{
			MapLayerBase mapLayer = VisualTreeHelper.GetParent(child) as MapLayerBase;
			if (mapLayer != null)
			{
				mapLayer.InvalidateArrange();
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			Map parentMap = GetParentMap();
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			foreach (UIElement element in Children)
			{
				element.Measure(availableSize);
			}

			return availableSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Clip = new RectangleGeometry { Rect = new Rect(0, 0, finalSize.Width, finalSize.Height) };

			foreach (UIElement element in Children)
			{
				ArrangeElement(element, finalSize);
			}

			return base.ArrangeOverride(finalSize);
		}

		protected void ArrangeElement(UIElement element, Size finalSize)
		{
			IHasLocation elementWithLocation = element as IHasLocation;
			Point? location;
			if (elementWithLocation != null)
			{
				location = elementWithLocation.Location;
			}
			else
			{
				location = GetLocationIfSet(element);
			}

			Map parentMap = GetParentMap();
			Point finalPosition;
			if (location.HasValue)
			{
				finalPosition = GetPositionForElementWithLocation(location.Value, element, parentMap);
			}
			else
			{
				finalPosition = GetPositionForElementWithoutLocation(element, finalSize);
			}

			Size elementDesiredSize = element.DesiredSize;

			bool rotateWithMap = GetRotateWithMap(element);
			if (rotateWithMap)
			{
				double rotation = parentMap.Heading;
				TransformGroup transform = GetMapTransformGroup(element);
				RotateTransform rotateTransform = transform.Children.OfType<RotateTransform>().FirstOrDefault();
				if (rotateTransform == null)
				{
					rotateTransform = new RotateTransform();
					transform.Children.Add(rotateTransform);
				}
				rotateTransform.Angle = rotation;
				rotateTransform.CenterX = elementDesiredSize.Width / 2;
				rotateTransform.CenterY = elementDesiredSize.Height / 2;
			}

			element.Arrange(new Rect(finalPosition, elementDesiredSize));
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
			Point position = parentMap.ViewPortProjection.ToCartesian(new Location(location.X, location.Y), parentMap.MapCenter.Longitude);

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

		private TransformGroup GetMapTransformGroup(UIElement element)
		{
			TransformGroup transform = element.RenderTransform as TransformGroup;

			if (transform == null)
			{
				transform = new TransformGroup();
				element.RenderTransform = transform;
			}

			return transform;
		}

		public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached("Location",
			typeof(Point),
			typeof(MapLayer),
			new PropertyMetadata(null, OnLocationPropertyChange));

		public static readonly DependencyProperty RotateWithMapProperty = DependencyProperty.RegisterAttached("RotateWithMap",
			typeof(bool),
			typeof(MapLayer),
			new PropertyMetadata(false));
	}
}