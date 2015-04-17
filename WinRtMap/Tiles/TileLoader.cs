using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WinRtMap.Tiles
{
	public class TileLoader
	{
		private readonly HttpClient _client = new HttpClient();
		private readonly ConcurrentBag<Tile> _tilesToLoad = new ConcurrentBag<Tile>();
		private volatile int _taskCount;

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
						Uri uri = new Uri(string.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", tile.Zoom, tile.X, tile.Y));
						using (HttpResponseMessage response = await _client.GetAsync(uri))
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
					catch (Exception e)
					{
						//If one Tile could not be donwloaded continue with the next.
						//TODO: Implement some proper Error Handling here. Retry?
					}
				}
				Interlocked.Decrement(ref _taskCount);
			});
		}
	}
}