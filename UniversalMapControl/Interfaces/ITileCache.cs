using System.Threading.Tasks;

using Windows.Storage.Streams;

namespace UniversalMapControl.Interfaces
{
	public interface ITileCache
	{
		Task<bool> TryLoadAsync(ITile tile);
		Task AddAsyc(ITile tile, IRandomAccessStream tileData);
	}
}