using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace WinRtMap.Utils
{
    public static class XamlHelper
    {
        /// <summary>
        /// LocationBinding Attached Dependency Property Helper
        /// This Attached Property can be used to help create a binding within a Property-Setter
        /// </summary>
        public static readonly DependencyProperty LocationBindingProperty = DependencyProperty.RegisterAttached("LocationBinding", typeof(string), typeof(XamlHelper), new PropertyMetadata(null, OnLocationBindingChanged));

        public static TAncestor GetAncestor<TAncestor>(this DependencyObject startingPoint) where TAncestor : class
        {
            DependencyObject parent = startingPoint;
            while (parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
                TAncestor found = parent as TAncestor;
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the LocationBinding property. This dependency property
        /// indicates the binding path for the MapLayerBase.LocationProperty binding.
        /// </summary>
        public static string GetLocationBinding(DependencyObject d)
        {
            return (string)d.GetValue(LocationBindingProperty);
        }

        /// <summary>
        /// Handles changes to the LocationBinding property.
        /// </summary>
        private static void OnLocationBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            string newBindingPath = (string)d.GetValue(LocationBindingProperty);
            FrameworkElement element = (FrameworkElement)d;
            element.SetBinding(MapLayerBase.LocationProperty, new Binding {Path = new PropertyPath(newBindingPath)});
        }

        /// <summary>
        /// Sets the LocationBinding property. This dependency property
        /// indicates the binding path for the MapLayerBase.LocationProperty binding.
        /// </summary>
        public static void SetLocationBinding(DependencyObject d, string value)
        {
            d.SetValue(LocationBindingProperty, value);
        }
    }
}