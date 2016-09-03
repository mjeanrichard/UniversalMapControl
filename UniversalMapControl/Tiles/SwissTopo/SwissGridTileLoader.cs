using Windows.Web.Http;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Tiles.Default;

namespace UniversalMapControl.Tiles.SwissTopo
{
    public class SwissGridTileLoader : DefaultWebTileLoader
    {
        public SwissGridTileLoader(ITileCache cache, ITiler tiler) : base(cache, tiler)
        {
        }

        public string LicenseKey { get; set; }

        protected override HttpRequestMessage BuildRequest(ICanvasBitmapTile tile)
        {
            HttpRequestMessage request = base.BuildRequest(tile);
            request.Headers.Add("Referer", LicenseKey);
            return request;
        }
    }
}