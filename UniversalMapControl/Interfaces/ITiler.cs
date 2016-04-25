using System;

using Windows.Foundation;

using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Projections;

namespace UniversalMapControl.Interfaces
{
	public interface ITiler
	{
		int GetTileSetForZoomFactor(double zoomFactor);
		bool IsPointOnValidTile(CartesianPoint point, int tileSet);
		CartesianPoint GetTilePositionForPoint(CartesianPoint point, int tileSet);
		Size GetTileSize(int tileSet);
		Uri GetUrl(ICanvasBitmapTile tile);
		ICanvasBitmapTile CreateTile(int tileSet, Rect tileBounds, CanvasControl canvas);
	}
}