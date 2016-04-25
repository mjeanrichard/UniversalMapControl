using System.Collections.Generic;

using Microsoft.Graphics.Canvas.UI.Xaml;

namespace UniversalMapControl.Interfaces
{
	public interface ITileProvider
	{
		void RefreshTiles(Map parentMap);
		IEnumerable<ICanvasBitmapTile> GetTiles(double zoomFactor);
		void ResetTiles(Map parentMap, CanvasControl canvas);
	}
}