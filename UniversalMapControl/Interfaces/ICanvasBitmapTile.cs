using Microsoft.Graphics.Canvas;

namespace UniversalMapControl.Interfaces
{
	public interface ICanvasBitmapTile : ITile
	{
		CanvasBitmap GetCanvasBitmap();
	}
}