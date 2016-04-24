using Microsoft.Graphics.Canvas;

namespace UniversalMapControl.Interfaces
{
	public interface ICanvasBitmapTile : ITile
	{
		ILocation Location { get; }
		CanvasBitmap GetCanvasBitmap();
	}
}