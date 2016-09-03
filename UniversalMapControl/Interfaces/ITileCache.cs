using System.Threading.Tasks;

using Windows.Storage.Streams;

namespace UniversalMapControl.Interfaces
{
    public interface ITileCache
    {
        Task<bool> TryLoadAsync(ICanvasBitmapTile tile);
        Task AddAsyc(ICanvasBitmapTile tile, IRandomAccessStream tileData);
        string LayerName { get; set; }
    }
}