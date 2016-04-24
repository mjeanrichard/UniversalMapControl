using Microsoft.Graphics.Canvas.UI.Xaml;

namespace UniversalMapControl.Interfaces
{
	public interface ILayerConfiguration
	{
		string LayerName { get; }
		string UrlPattern { get; }

		ITileProvider TileProvider { get; }
		ITileLoader TileLoader { get; }
		ITileCache TileCache { get; }
		ICanvasBitmapTile CreateTile(int x, int y, int zoom, ILocation location);
		void SetCanvas(CanvasControl canvas);
	}
}