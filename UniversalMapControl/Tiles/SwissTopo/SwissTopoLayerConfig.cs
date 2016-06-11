using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles.SwissTopo
{
	public class SwissTopoLayerConfig : ILayerConfiguration
	{
		private readonly SwissGridTileLoader _tileLoader;
		private ITileCache _cache;

		public SwissTopoLayerConfig()
		{
			_cache = new FileSystemTileCache();
			LayerName = "SwissTopo";

			ITiler tiler = new SwissTopoTiler();
			_tileLoader = new SwissGridTileLoader(_cache, tiler);
			TileProvider = new TileProvider(tiler, _tileLoader);
		}

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
			get { return _cache.LayerName; }
			set { _cache.LayerName = value; }
		}

		public ITileProvider TileProvider { get; private set; }
	}
}