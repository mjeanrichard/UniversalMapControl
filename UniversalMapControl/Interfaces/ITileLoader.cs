using System.ComponentModel;

namespace UniversalMapControl.Interfaces
{
	public interface ITileLoader : INotifyPropertyChanged
	{
		void Enqueue(ICanvasBitmapTile tile);
		int PendingTileCount { get; }
	}
}