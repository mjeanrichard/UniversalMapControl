using Windows.Foundation;
using Windows.UI.Xaml;
using WinRtMap.Utils;

namespace WinRtMap.Tiles
{
	public abstract class BaseTile
	{
		public abstract UIElement Element { get; }
		public abstract Location Location { get; }
	}
}