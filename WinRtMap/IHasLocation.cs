using Windows.Foundation;

namespace WinRtMap
{
	public interface IHasLocation
	{
		Point Location { get; }
	}
}