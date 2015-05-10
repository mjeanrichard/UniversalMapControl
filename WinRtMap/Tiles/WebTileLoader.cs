using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace WinRtMap.Tiles
{
    public class WebTileLoader : BaseTileLoader<WebTile>
    {
        private readonly HttpClient _client;

        public WebTileLoader()
        {
            _client = new HttpClient();
        }

        protected override async Task<InMemoryRandomAccessStream> LoadTileImage(WebTile tile)
        {
            using (HttpRequestMessage tileRequest = BuildRequest(tile))
            {
                using (HttpResponseMessage response = await _client.SendRequestAsync(tileRequest))
                {
                    InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
                    await response.Content.WriteToStreamAsync(ras);
                    ras.Seek(0);
                    return ras;
                }
            }
        }

        protected virtual HttpRequestMessage BuildRequest(WebTile tile)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, tile.Url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3)");
            return request;
        }
    }
}