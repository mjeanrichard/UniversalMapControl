using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace WinRtMap.Utils
{
	public static class XamlHelper
	{
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
	}
}