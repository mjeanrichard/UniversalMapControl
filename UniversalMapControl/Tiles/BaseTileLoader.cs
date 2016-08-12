using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage.Streams;
using Windows.System.Threading;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Utils;

namespace UniversalMapControl.Tiles
{
	public abstract class BaseTileLoader : ITileLoader
	{
		private readonly ITileCache _tileCache;

		private readonly Dictionary<int, List<ICanvasBitmapTile>> _tilesToLoad = new Dictionary<int, List<ICanvasBitmapTile>>();

		private readonly object _locker = new object();

		private int _taskCount;

		private int _pendingTileCount = 0;

		protected BaseTileLoader(ITileCache tileCache)
		{
			_tileCache = tileCache;
			MaxParallelTasks = 5;
#if DEBUG
			MaxParallelTasks = 1;
#endif
		}

		protected abstract Task<InMemoryRandomAccessStream> LoadTileImage(ICanvasBitmapTile tile);

		public event PropertyChangedEventHandler PropertyChanged;

		public void Enqueue(ICanvasBitmapTile tile)
		{
			if (!tile.HasImage)
			{
				lock(_locker)
				{
					List<ICanvasBitmapTile> tiles;
					if (!_tilesToLoad.TryGetValue(tile.TileSet, out tiles))
					{
						tiles = new List<ICanvasBitmapTile>();
						_tilesToLoad.Add(tile.TileSet, tiles);
					}
					tiles.Add(tile);
					Interlocked.Increment(ref _pendingTileCount);
				}
				StartDownloading();
				OnPropertyChanged(nameof(PendingTileCount));
			}
		}

		public int PendingTileCount
		{
			get { return _pendingTileCount; }
		}

		public int MaxParallelTasks { get; set; }

		private void StartDownloading()
		{
			if (_taskCount >= MaxParallelTasks)
			{
				MapEventSource.Log.TileLoaderMaxTasksRunning(_taskCount);
				return;
			}
			Interlocked.Increment(ref _taskCount);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			ThreadPool.RunAsync(o =>
			{
#if ENABLE_ETW_ACTIVITY_SCOPE
				using (MapEventSource.StartActivityScope())
				{
#endif
					try
					{
						MapEventSource.Log.TileLoaderTaskStarting(_taskCount);
						RetrieveTiles();
					}
					finally
					{
						Interlocked.Decrement(ref _taskCount);
						MapEventSource.Log.TileLoaderTaskCompleted(_taskCount);
					}
#if ENABLE_ETW_ACTIVITY_SCOPE
				}
#endif
			});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		private bool GetNextTile(out ICanvasBitmapTile tile)
		{
			lock(_locker)
			{
				tile = null;
				while (_tilesToLoad.Any())
				{
					int minKey = _tilesToLoad.Keys.Min();
					List<ICanvasBitmapTile> tileForMinLevel = _tilesToLoad[minKey];
					if (tileForMinLevel.Any())
					{
						tile = tileForMinLevel.FirstOrDefault(t => !t.IsNotInCache);
						if (tile == null)
						{
							tile = tileForMinLevel.First();
						}
						tileForMinLevel.Remove(tile);
						Interlocked.Decrement(ref _pendingTileCount);
						break;
					}
					_tilesToLoad.Remove(minKey);
				}
			}
			OnPropertyChanged(nameof(PendingTileCount));
			return tile != null;
		}

		private void RetrieveTiles()
		{
			ICanvasBitmapTile tile;
			while (GetNextTile(out tile))
			{
				MapEventSource.Log.TileLoaderRetrieveStarted(tile.CacheKey);
				if (tile.HasImage || tile.IsDisposed)
				{
					MapEventSource.Log.TileLoaderRetrieveCompleted(tile.CacheKey, tile.HasImage, tile.IsDisposed);
					continue;
				}
				try
				{
					if (_tileCache != null)
					{
						if (!_tileCache.TryLoadAsync(tile).Result && !tile.IsNotInCache)
						{
							MapEventSource.Log.TileLoaderCacheMiss(tile.CacheKey);
							tile.IsNotInCache = true;
							Enqueue(tile);
							return;
						}
						MapEventSource.Log.TileLoaderCacheHit(tile.CacheKey);
					}
					if (!tile.HasImage)
					{
						MapEventSource.Log.TileLoaderLoadTileStarted(tile.CacheKey);
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
						MapEventSource.Log.TileLoaderLoadTileCompleted(tile.CacheKey);
					}
				}
				catch (Exception e)
				{
					MapEventSource.Log.TileLoaderRetrieveException(tile.CacheKey, e.ToString());
					//If one Tile could not be downloaded add it back to the Bag.
					//if (!tile.HasImage && !tile.IsDisposed)
					//{
					// _tilesToLoad.Add(tile);
					//}
				}
				MapEventSource.Log.TileLoaderRetrieveCompleted(tile.CacheKey, tile.HasImage, tile.IsDisposed);
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}