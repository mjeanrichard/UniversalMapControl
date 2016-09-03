using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles.Default
{
    public class DefaultWebLayerConfig : ILayerConfiguration
    {
        private readonly DefaultWebTiler _tiler;

        public DefaultWebLayerConfig()
        {
            TileCache = new FileSystemTileCache();
            _tiler = new DefaultWebTiler();
            TileLoader = new DefaultWebTileLoader(TileCache, _tiler);
            TileProvider = new TileProvider(_tiler, TileLoader);

            UrlPattern = "http://{RND-a;b;c}.tile.openstreetmap.org/{z}/{x}/{y}.png";
            LayerName = "OSM";
        }

        /// <summary>
        /// URL Patter to load the Tile from. The following Patter are supported:
        /// {x}/{y}/{z} : Coordinates
        /// {RND-a;b;c} : Randomly picks one of the supplied values (separated by semicolon)
        /// </summary>
        public string UrlPattern
        {
            get { return _tiler.UrlPattern; }
            set { _tiler.UrlPattern = value; }
        }

        public ITileCache TileCache { get; set; }

        public ITileLoader TileLoader { get; }

        /// <summary>
        /// Name of the Layer. This is used to create a unique folder for the Filesystem Cache.
        /// </summary>
        public string LayerName { get; set; }

        public ITileProvider TileProvider { get; private set; }
    }
}