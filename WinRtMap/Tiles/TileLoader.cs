using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WinRtMap.Tiles
{
	public class TileLoader
	{
		private readonly HttpClient _client;
		private readonly ConcurrentBag<Tile> _tilesToLoad = new ConcurrentBag<Tile>();
		private volatile int _taskCount;

		public TileLoader()
		{
			_client = new HttpClient();
		}

		public void Enqueue(Tile tile)
		{
			if (!tile.HasImage)
			{
				_tilesToLoad.Add(tile);
				StartDownloading();
			}
		}

		private void StartDownloading()
		{
			if (_taskCount >= 5)
			{
				return;
			}
			Interlocked.Increment(ref _taskCount);

			Task.Run(async () =>
			{
				Tile tile;
				while (_tilesToLoad.TryTake(out tile))
				{
					if (tile.HasImage || tile.IsRemoved)
					{
						continue;
					}
					try
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
					catch (Exception e)
					{
						//If one Tile could not be donwloaded continue with the next.
						//TODO: Implement some proper Error Handling here. Retry?
					}
				}
				Interlocked.Decrement(ref _taskCount);
			});
		}

		protected virtual HttpRequestMessage BuildRequest(Tile tile)
		{
			Uri uri = new Uri(string.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", tile.Zoom, tile.X, tile.Y));
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
			request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36");
            return request;
		}
	}
}