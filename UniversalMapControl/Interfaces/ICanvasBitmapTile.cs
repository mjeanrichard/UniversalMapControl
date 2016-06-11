using System.Threading.Tasks;

using Windows.Storage.Streams;

using Microsoft.Graphics.Canvas;

namespace UniversalMapControl.Interfaces
{
	public interface ICanvasBitmapTile : ITile
	{
		CanvasBitmap GetCanvasBitmap();
		bool HasImage { get; }
		bool IsNotInCache { get; set; }
		Task ReadFromAsync(IRandomAccessStream imageStream);
	}
}