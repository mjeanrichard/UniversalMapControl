using System;

using Windows.Foundation;
using Windows.UI;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Tiles.Default;
using UniversalMapControl.Utils;

namespace UniversalMapControl.Tiles
{
    public class TileLayer : CanvasMapLayer
    {
        private ILayerConfiguration _layerConfiguration;
        private bool _isInitialResourcesLoaded = false;
        private bool _isSpriteBatchSupported = false;

        public TileLayer()
        {
            LayerConfiguration = new DefaultWebLayerConfig();
            ShowLoadingOverlay = false;
            LoadingOverlayColor = Color.FromArgb(100, 150, 150, 150);
        }

        public ILayerConfiguration LayerConfiguration
        {
            get { return _layerConfiguration; }
            set
            {
                _layerConfiguration = value;
                if ((Canvas != null) && _isInitialResourcesLoaded)
                {
                    _layerConfiguration.TileProvider.ResetTiles(ParentMap, Canvas);
                }
            }
        }

        public bool ShowLoadingOverlay { get; set; }

        public Color LoadingOverlayColor { get; set; }

        protected override void OnCreateResource(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            // Clear all Tiles and Reload (Display Device might have changed...)

            _isSpriteBatchSupported = CanvasSpriteBatch.IsSupported(sender.Device);
            LayerConfiguration.TileProvider.ResetTiles(ParentMap, sender);
            _isInitialResourcesLoaded = true;
        }

        protected override void DrawInternal(CanvasDrawingSession drawingSession, Map parentMap)
        {
            LayerConfiguration.TileProvider.RefreshTiles(ParentMap);
            double zoomFactor = parentMap.ViewPortProjection.GetZoomFactor(parentMap.ZoomLevel);

            using (BatchSpriteWrapper spriteBatch = BatchSpriteWrapper.Create(_isSpriteBatchSupported, drawingSession))
            {
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
                        Rect scale = Scale(tile.Bounds);
                        spriteBatch.Draw(canvasBitmap, scale);
                    }
                    else if (ShowLoadingOverlay)
                    {
                        DrawLoadingTile(drawingSession, tile, parentMap);
                    }
                }
            }
        }

        protected virtual void DrawInexistentTile(CanvasDrawingSession drawingSession, ICanvasBitmapTile tile, Map parentMap)
        {
        }

        protected virtual void DrawLoadingTile(CanvasDrawingSession drawingSession, ICanvasBitmapTile tile, Map parentMap)
        {
            drawingSession.FillRectangle(tile.Bounds, LoadingOverlayColor);
        }
    }
}