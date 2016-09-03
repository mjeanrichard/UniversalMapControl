using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles.SwissTopo
{
    public class SwissTopoLayerConfig : ILayerConfiguration
    {
        private readonly SwissGridTileLoader _tileLoader;

        public SwissTopoLayerConfig()
        {
            TileCache = new FileSystemTileCache();
            LayerName = "SwissTopo";

            ITiler tiler = new SwissTopoTiler();
            _tileLoader = new SwissGridTileLoader(TileCache, tiler);
            TileProvider = new TileProvider(tiler, _tileLoader);
        }

        public ITileCache TileCache { get; set; }

        public string LicenseKey
        {
            get { return _tileLoader.LicenseKey; }
            set { _tileLoader.LicenseKey = value; }
        }

        public ITileLoader TileLoader
        {
            get { return _tileLoader; }
        }

        /// <summary>
        /// Name of the Layer. This is used to create a unique folder for the Filesystem Cache.
        /// </summary>
        public string LayerName
        {
            get { return TileCache.LayerName; }
            set { TileCache.LayerName = value; }
        }

        public ITileProvider TileProvider { get; private set; }
    }
}