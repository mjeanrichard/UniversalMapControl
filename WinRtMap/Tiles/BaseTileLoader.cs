using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WinRtMap.Tiles
{
	public abstract class BaseTileLoader<TTile> where TTile : BaseTile
	{
		private readonly ConcurrentBag<TTile> _tilesToLoad = new ConcurrentBag<TTile>();
		private volatile int _taskCount;

		protected BaseTileLoader()
		{}

		public void Enqueue(TTile tile)
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
				TTile tile;
				while (_tilesToLoad.TryTake(out tile))
				{
					if (tile.HasImage || tile.IsRemoved)
					{
						continue;
					}
					try
					{
						await LoadTile(tile);
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

		protected abstract Task LoadTile(TTile tile);
	}
}