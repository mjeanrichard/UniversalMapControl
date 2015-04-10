using Windows.Foundation;
using Windows.UI.Xaml;

namespace WinRtMap.Tiles
{
	public abstract class BaseTile
	{
		public abstract UIElement Element { get; }
		public abstract Point Location { get; }
	}
}