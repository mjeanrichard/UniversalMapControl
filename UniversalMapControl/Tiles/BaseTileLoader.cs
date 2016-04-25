using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage.Streams;
using Windows.System.Threading;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles
{
	public abstract class BaseTileLoader : ITileLoader
	{
		private readonly ITileCache _tileCache;
		private readonly ConcurrentBag<ICanvasBitmapTile> _tilesToLoad = new ConcurrentBag<ICanvasBitmapTile>();
		private int _taskCount;

		protected BaseTileLoader(ITileCache tileCache)
		{
			_tileCache = tileCache;
			MaxParallelTasks = 5;
#if DEBUG
			MaxParallelTasks = 1;
#endif
		}

		public void Enqueue(ICanvasBitmapTile tile)
		{
			if (!tile.HasImage)
			{
				_tilesToLoad.Add(tile);
				StartDownloading();
			}
		}

		public int MaxParallelTasks { get; set; }

		private void StartDownloading()
		{
			if (_taskCount >= MaxParallelTasks)
			{
				return;
			}
			Interlocked.Increment(ref _taskCount);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			ThreadPool.RunAsync(o =>
			{
				RetrieveTiles();
				Interlocked.Decrement(ref _taskCount);
			});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		private void RetrieveTiles()
		{
			ICanvasBitmapTile tile;
			while (_tilesToLoad.TryTake(out tile))
			{
				if (tile.HasImage || tile.IsDisposed)
				{
					continue;
				}
				try
				{
					if (_tileCache != null)
					{
						_tileCache.TryLoadAsync(tile).Wait();
					}
					if (!tile.HasImage)
					{
						InMemoryRandomAccessStream imageStream = LoadTileImage(tile).Result;
						if (imageStream == null)
						{
							continue;
						}
						using (imageStream)
						{
							if (imageStream.Size > 0)
							{
								tile.ReadFromAsync(imageStream).Wait();
								if (_tileCache != null)
								{
									imageStream.Seek(0);
									_tileCache.AddAsyc(tile, imageStream).Wait();
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					//If one Tile could not be downloaded add it back to the Bag.
					//if (!tile.HasImage && !tile.IsDisposed)
					//{
					// _tilesToLoad.Add(tile);
					//}
				}
			}
		}

		protected abstract Task<InMemoryRandomAccessStream> LoadTileImage(ICanvasBitmapTile tile);
	}
}