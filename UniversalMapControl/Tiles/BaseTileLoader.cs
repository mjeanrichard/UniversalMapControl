using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace UniversalMapControl.Tiles
{
    public abstract class BaseTileLoader<TTile> where TTile : BaseTile
    {
        private readonly ITileCache _cache;
        private readonly ConcurrentBag<TTile> _tilesToLoad = new ConcurrentBag<TTile>();
        private int _taskCount;

        protected BaseTileLoader() : this(new FileSystemTileCache())
        {}

        protected BaseTileLoader(ITileCache cache)
        {
            _cache = cache;
        }

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
            TTile tile;
            while (_tilesToLoad.TryTake(out tile))
            {
                if (tile.HasImage || tile.IsDisposed)
                {
                    continue;
                }
                try
                {
                    if (_cache != null)
                    {
                        using (IRandomAccessStream cacheStream = _cache.TryGetStream(tile).Result)
                        {
                            if (cacheStream != null)
                            {
                                tile.SetImage(cacheStream);
                            }
                        }
                    }
                    if (!tile.HasImage)
                    {
                        using (InMemoryRandomAccessStream imageStream = LoadTileImage(tile).Result)
                        {
                            tile.SetImage(imageStream);
                            if (_cache != null)
                            {
                                imageStream.Seek(0);
                                _cache.Add(tile, imageStream).Wait();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //If one Tile could not be downloaded add it back to the Bag.
	                if (!tile.HasImage && !tile.IsDisposed)
	                {
		                _tilesToLoad.Add(tile);
	                }
                }
            }
        }

        protected abstract Task<InMemoryRandomAccessStream> LoadTileImage(TTile tile);
    }
}