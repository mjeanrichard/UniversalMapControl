using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles
{
    public interface ILayerConfiguration
    {
        ITileProvider TileProvider { get; }
        ITileLoader TileLoader { get; }
        string LayerName { get; }
        ITileCache TileCache { get; }
    }
}