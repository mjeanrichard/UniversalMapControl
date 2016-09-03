using System;
using System.Diagnostics.Tracing;

namespace UniversalMapControl.Utils
{
    [EventSource(Name = "UniversalMapControl")]
    internal sealed class MapEventSource : EventSource
    {
        public static MapEventSource Log = new MapEventSource();

#if ENABLE_ETW_ACTIVITY_SCOPE
		public static IDisposable StartActivityScope()
		{
			return new ActivityScope();
		}
#endif

        // Tile Loader
        [Event(500, Message = "Starting a new Downloader Task ({0} Tasks are now running).", Level = EventLevel.Verbose)]
        public void TileLoaderTaskStarting(int taskCount)
        {
            WriteEvent(500, taskCount);
        }

        [Event(501, Message = "Downloader Task completed ({0} Tasks still running).", Level = EventLevel.Verbose)]
        public void TileLoaderTaskCompleted(int taskCount)
        {
            WriteEvent(501, taskCount);
        }

        [Event(502, Message = "There are already {0} Tasks running. No new Task is started.", Level = EventLevel.Verbose)]
        public void TileLoaderMaxTasksRunning(int taskCount)
        {
            WriteEvent(502, taskCount);
        }

        [Event(510, Message = "Starting Download of Tile.", Level = EventLevel.Informational, Task = Tasks.TileLoaderDownloadTile, Opcode = EventOpcode.Start)]
        public void TileLoaderLoadTileStarted(string key)
        {
            WriteEvent(510, key);
        }

        [Event(511, Message = "Download of Tile completed.", Level = EventLevel.Informational, Task = Tasks.TileLoaderDownloadTile, Opcode = EventOpcode.Stop)]
        public void TileLoaderLoadTileCompleted(string key)
        {
            WriteEvent(511, key);
        }

        [Event(512, Message = "Retrieve of Tile '{0}' started.", Level = EventLevel.Informational, Task = Tasks.TileLoaderRetrieveTile, Opcode = EventOpcode.Start)]
        public void TileLoaderRetrieveStarted(string key)
        {
            WriteEvent(512, key);
        }

        [Event(513, Message = "Retrieve of Tile '{0}' completed.", Level = EventLevel.Informational, Task = Tasks.TileLoaderRetrieveTile, Opcode = EventOpcode.Stop)]
        public void TileLoaderRetrieveCompleted(string key, bool hasImage, bool isDisposed)
        {
            WriteEvent(513, key, hasImage, isDisposed);
        }

        [Event(514, Message = "Loading Tile '{0}' from cache.", Level = EventLevel.Verbose)]
        public void TileLoaderCacheHit(string key)
        {
            WriteEvent(514, key);
        }

        [Event(515, Message = "Tile '{0}' is not available from the cache.", Level = EventLevel.Verbose)]
        public void TileLoaderCacheMiss(string key)
        {
            WriteEvent(515, key);
        }

        [Event(520, Message = "Exception while retrieving Tile '{0}'.", Level = EventLevel.Warning)]
        public void TileLoaderRetrieveException(string key, string exception)
        {
            WriteEvent(520, key, exception);
        }

        [Event(530, Message = "Download of Tile '{0}' failed with Status Code {1}.", Level = EventLevel.Warning)]
        public void TileLoaderDownloadFailed(string key, int statusCode, string url)
        {
            WriteEvent(530, key, statusCode, url);
        }

        private class ActivityScope : IDisposable
        {
            private readonly Guid _oldActivityId;

            public ActivityScope()
            {
                _oldActivityId = CurrentThreadActivityId;
                SetCurrentThreadActivityId(Guid.NewGuid());
            }

            public void Dispose()
            {
                SetCurrentThreadActivityId(_oldActivityId);
            }
        }

        public class Tasks
        {
            public const EventTask TileLoaderDownloadTile = (EventTask)1;
            public const EventTask TileLoaderRetrieveTile = (EventTask)2;
        }
    }
}