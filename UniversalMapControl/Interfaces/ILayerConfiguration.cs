using Microsoft.Graphics.Canvas.UI.Xaml;

namespace UniversalMapControl.Interfaces
{
	public interface ILayerConfiguration
	{
		string LayerName { get; }
		string UrlPattern { get; }

		ITileProvider TileProvider { get; }
		ITileLoader TileLoader { get; }
		IProjection Projection { get; }
		ITileCache TileCache { get; }
		ICanvasBitmapTile CreateTile(int x, int y, int tileSet, ILocation location);
		void SetCanvas(CanvasControl canvas);
	}
}