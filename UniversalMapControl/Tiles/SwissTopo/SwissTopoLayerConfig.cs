using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles.SwissTopo
{
	public class SwissTopoLayerConfig : ILayerConfiguration
	{
		private readonly SwissGridTileLoader _tileLoader;

		public SwissTopoLayerConfig()
		{
			LayerName = "SwissTopo";

			ITileCache cache = null;
			ITiler tiler = new SwissTopoTiler();
			_tileLoader = new SwissGridTileLoader(cache, tiler);
			TileProvider = new TileProvider(tiler, _tileLoader);
		}

		public string LicenseKey
		{
			get { return _tileLoader.LicenseKey; }
			set { _tileLoader.LicenseKey = value; }
		}

		/// <summary>
		/// Name of the Layer. This is used to create a unique folder for the Filesystem Cache.
		/// </summary>
		public string LayerName { get; set; }

		public ITileProvider TileProvider { get; private set; }
	}
}