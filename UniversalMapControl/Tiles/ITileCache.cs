using System.Threading.Tasks;

using Windows.Storage.Streams;

namespace UniversalMapControl.Tiles
{
	public interface ITileCache
	{
		Task<IRandomAccessStream> TryGetStream(BaseTile tile);
		Task Add(BaseTile tile, IInputStream imageStream);
	}
}