using Microsoft.Graphics.Canvas;

namespace UniversalMapControl.Interfaces
{
	public interface ICanvasBitmapTile : ITile
	{
		Location Location { get; }
		CanvasBitmap GetCanvasBitmap();
	}
}