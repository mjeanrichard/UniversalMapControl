using System;
using System.Threading.Tasks;

using Windows.Storage.Streams;

namespace UniversalMapControl.Interfaces
{
	public interface ITile : IDisposable
	{
		bool HasImage { get; }
		bool IsDisposed { get; }
		bool IsCachable { get; set; }
		int X { get; }
		int Y { get; }
		int Zoom { get; }

		Task ReadFromAsync(IRandomAccessStream imageStream);
	}
}