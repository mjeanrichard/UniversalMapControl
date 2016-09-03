using System;

using Windows.Foundation;

namespace UniversalMapControl.Interfaces
{
    public enum TileState
    {
        LoadPending,
        Loaded,
        TileDoesNotExist
    }

    public interface ITile : IDisposable
    {
        bool IsDisposed { get; }
        Rect Bounds { get; }
        int TileSet { get; }

        TileState State { get; set; }

        string CacheKey { get; }
    }
}