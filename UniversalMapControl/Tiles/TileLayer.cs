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
		}

		public ILayerConfiguration LayerConfiguration { get; set; }

		protected override void OnCreateResource(CanvasControl sender, CanvasCreateResourcesEventArgs args)
		{
			// Clear all Tiles and Reload (Display Device might have changed...)
			LayerConfiguration.TileProvider.ResetTiles(ParentMap, sender);
		}

		protected override void DrawInternal(CanvasDrawingSession drawingSession, Map parentMap)
		{
			LayerConfiguration.TileProvider.RefreshTiles(ParentMap);

			foreach (ICanvasBitmapTile tile in LayerConfiguration.TileProvider.GetTiles(parentMap.ZoomLevel))
			{
				CanvasBitmap canvasBitmap = tile.GetCanvasBitmap();
				if (canvasBitmap != null)
				{
					drawingSession.DrawImage(canvasBitmap, tile.Bounds);
				}
			}
		}
	}
}