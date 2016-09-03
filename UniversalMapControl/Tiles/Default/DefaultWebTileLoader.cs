using System;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Utils;

namespace UniversalMapControl.Tiles.Default
{
    public class DefaultWebTileLoader : BaseTileLoader
    {
        private readonly ITiler _tiler;
        private readonly HttpClient _client;

        public DefaultWebTileLoader(ITileCache cache, ITiler tiler) : base(cache)
        {
            _tiler = tiler;
            _client = new HttpClient();
        }

        protected override async Task<InMemoryRandomAccessStream> LoadTileImage(ICanvasBitmapTile tile)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return await CreateEmptyImage();
            }
            using (HttpRequestMessage tileRequest = BuildRequest(tile))
            {
                using (HttpResponseMessage response = await _client.SendRequestAsync(tileRequest))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        MapEventSource.Log.TileLoaderDownloadFailed(tile.CacheKey, (int)response.StatusCode, tileRequest.RequestUri.ToString());
                        return null;
                    }
                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        tile.State = TileState.TileDoesNotExist;
                        return null;
                    }
                    InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
                    await response.Content.WriteToStreamAsync(ras);
                    ras.Seek(0);
                    tile.State = TileState.Loaded;
                    return ras;
                }
            }
        }

        private static async Task<InMemoryRandomAccessStream> CreateEmptyImage()
        {
            Uri appUri = new Uri("ms-appx:///Assets/EmptyTile.png");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(appUri).AsTask().ConfigureAwait(false);
            IBuffer buffer = await FileIO.ReadBufferAsync(file).AsTask().ConfigureAwait(false);
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(buffer).AsTask().ConfigureAwait(false);
            stream.Seek(0);
            return stream;
        }

        protected virtual HttpRequestMessage BuildRequest(ICanvasBitmapTile tile)
        {
            Uri url = _tiler.GetUrl(tile);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3)");
            return request;
        }
    }
}