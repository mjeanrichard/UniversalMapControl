using Windows.Foundation;
using Windows.UI.Xaml;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles
{
	public class TileLayer : CanvasMapLayer
	{
		public ILayerConfiguration Configuration { get; set; }

		public TileLayer()
		{
			Configuration = new LayerConfiguration();
		}

		protected override void OnLayerLoaded(object sender, RoutedEventArgs e)
		{
			base.OnLayerLoaded(sender, e);
			Configuration.SetCanvas(Canvas);
		}

		protected override void OnCreateResource(CanvasControl sender, CanvasCreateResourcesEventArgs args)
		{
			// Clear all Tiles and Reload (Display Device might have changed...)
			Configuration.TileProvider.ResetTiles();
			Configuration.TileProvider.RefreshTiles(ParentMap);
		}

		protected override void DrawInternal(CanvasDrawingSession drawingSession, Map parentMap)
		{
			Configuration.TileProvider.RefreshTiles(ParentMap);

			foreach (ICanvasBitmapTile tile in Configuration.TileProvider.GetTiles(parentMap.ZoomLevel))
			{
				Point position = parentMap.ViewPortProjection.ToCartesian(tile.Location, false);

				CanvasBitmap canvasBitmap = tile.GetCanvasBitmap();
				if (canvasBitmap != null)
				{
					double z = parentMap.ViewPortProjection.GetZoomFactor(tile.Zoom);
					double width = canvasBitmap.Size.Width / z;
					double height = canvasBitmap.Size.Height / z;
					Rect dest = new Rect((float)position.X, (float)position.Y, width, height);
					drawingSession.DrawImage(canvasBitmap, dest);
				}
			}
		}
	}
}