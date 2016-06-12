using Windows.UI;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Tiles.Default;

namespace UniversalMapControl.Tiles
{
	public class TileLayer : CanvasMapLayer
	{
		public TileLayer()
		{
			LayerConfiguration = new DefaultWebLayerConfig();
			ShowLoadingOverlay = false;
			LoadingOverlayColor = Color.FromArgb(100, 150, 150, 150);
		}

		public ILayerConfiguration LayerConfiguration { get; set; }

		public bool ShowLoadingOverlay { get; set; }

		public Color LoadingOverlayColor { get; set; }

		protected override void OnCreateResource(CanvasControl sender, CanvasCreateResourcesEventArgs args)
		{
			// Clear all Tiles and Reload (Display Device might have changed...)
			LayerConfiguration.TileProvider.ResetTiles(ParentMap, sender);
		}

		protected override void DrawInternal(CanvasDrawingSession drawingSession, Map parentMap)
		{
			drawingSession.Antialiasing = CanvasAntialiasing.Aliased;

			LayerConfiguration.TileProvider.RefreshTiles(ParentMap);
			double zoomFactor = parentMap.ViewPortProjection.GetZoomFactor(parentMap.ZoomLevel);

			foreach (ICanvasBitmapTile tile in LayerConfiguration.TileProvider.GetTiles(zoomFactor))
			{
				if (tile.State == TileState.TileDoesNotExist)
				{
					DrawInexistentTile(drawingSession, tile, parentMap);
					continue;
				}
				CanvasBitmap canvasBitmap = tile.GetCanvasBitmap();
				if (canvasBitmap != null)
				{
					drawingSession.DrawImage(canvasBitmap, tile.Bounds);
				}
				else if (ShowLoadingOverlay)
				{
					DrawLoadingTile(drawingSession, tile, parentMap);
				}
			}
		}

		protected virtual void DrawInexistentTile(CanvasDrawingSession drawingSession, ICanvasBitmapTile tile, Map parentMap)
		{}

		protected virtual void DrawLoadingTile(CanvasDrawingSession drawingSession, ICanvasBitmapTile tile, Map parentMap)
		{
			drawingSession.FillRectangle(tile.Bounds, LoadingOverlayColor);
		}
	}
}