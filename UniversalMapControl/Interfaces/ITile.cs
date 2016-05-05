using System;

using Windows.Foundation;

namespace UniversalMapControl.Interfaces
{
	public interface ITile : IDisposable
	{
		bool IsDisposed { get; }
		Rect Bounds { get; }
		int TileSet { get; }

		bool IsCachable { get; set; }

		string CacheKey { get; }
	}
}