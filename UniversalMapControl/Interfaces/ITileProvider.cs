using System.Collections.Generic;

using Microsoft.Graphics.Canvas.UI.Xaml;

namespace UniversalMapControl.Interfaces
{
    public interface ITileProvider
    {
        void RefreshTiles(Map parentMap);
        IEnumerable<ICanvasBitmapTile> GetTiles(double zoomFactor);
        void ResetTiles(Map parentMap, CanvasControl canvas);

        /// <summary>
        /// Specifies how many lower zoom level should automatically be loaded.
        /// Use 0 to disable loading of lower layers, use int.MaxValue to load all lower levels.
        /// Default is int.MaxValue.
        /// </summary>
        int LowerTileSetsToLoad { get; set; }
    }
}