using System.Collections.Generic;

namespace UniversalMapControl.Interfaces
{
	public interface ITileProvider
	{
		void RefreshTiles(Map parentMap);
		IEnumerable<ICanvasBitmapTile> GetTiles(double zoomLevel);
		void ResetTiles();
	}
}