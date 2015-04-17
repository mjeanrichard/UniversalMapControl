using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WinRtMap.Tiles
{
	public class WebTileLoader : BaseTileLoader<WebTile>
	{
		private readonly HttpClient _client;

		public WebTileLoader()
		{
			_client = new HttpClient();
		}

		protected override async Task LoadTile(WebTile tile)
		{
			using (HttpRequestMessage tileRequest = BuildRequest(tile))
			{
				using (HttpResponseMessage response = await _client.SendAsync(tileRequest))
				{
					using (MemoryStream memStream = new MemoryStream())
					{
						await response.Content.CopyToAsync(memStream);
						memStream.Position = 0;
						using (IRandomAccessStream ras = memStream.AsRandomAccessStream())
						{
							await tile.SetImage(ras);
						}
					}
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