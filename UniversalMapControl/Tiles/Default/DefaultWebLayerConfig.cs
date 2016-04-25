using UniversalMapControl.Interfaces;
using UniversalMapControl.Tiles.SwissTopo;

namespace UniversalMapControl.Tiles.Default
{
	public class DefaultWebLayerConfig : ILayerConfiguration
	{
		private readonly DefaultWebTiler _tiler;

		public DefaultWebLayerConfig()
		{
			ITileCache cache = null;
			_tiler = new DefaultWebTiler();
			ITileLoader tileLoader = new DefaultWebTileLoader(cache, _tiler);
			TileProvider = new TileProvider(_tiler, tileLoader);

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

		/// <summary>
		/// Name of the Layer. This is used to create a unique folder for the Filesystem Cache.
		/// </summary>
		public string LayerName { get; set; }

		public ITileProvider TileProvider { get; private set; }
	}
}