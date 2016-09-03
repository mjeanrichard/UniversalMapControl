using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;

using UniversalMapControl.Interfaces;

using Buffer = Windows.Storage.Streams.Buffer;

namespace UniversalMapControl.Tiles
{
    public class FileSystemTileCache : ITileCache
    {
        public string LayerName { get; set; }

        public async Task<IRandomAccessStream> TryGetStream(ITile tile)
        {
            StorageFolder folder = await OpenFolder(tile).ConfigureAwait(false);
            string filename = string.Format(CultureInfo.InvariantCulture, "{0}.tile", tile.CacheKey);
            IStorageItem storageItem = await folder.TryGetItemAsync(filename).AsTask().ConfigureAwait(false);
            if (storageItem == null)
            {
                return null;
            }

            StorageFile file;
            try
            {
                file = await folder.GetFileAsync(filename).AsTask().ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                return null;
            }

            IRandomAccessStreamWithContentType stream = await file.OpenReadAsync().AsTask().ConfigureAwait(false);
            return stream;
        }

        private async Task<StorageFolder> OpenFolder(ITile tile)
        {
            ApplicationData appData = ApplicationData.Current;
            string folderName = string.Format(CultureInfo.InvariantCulture, "UMCCache\\{0}\\{1}", LayerName, tile.TileSet);
            return await appData.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
        }

        public async Task<bool> TryLoadAsync(ICanvasBitmapTile tile)
        {
            IRandomAccessStream stream = await TryGetStream(tile);
            if ((stream == null) || (stream.Size == 0))
            {
                return false;
            }
            await tile.ReadFromAsync(stream);
            return tile.HasImage;
        }

        public async Task AddAsyc(ICanvasBitmapTile tile, IRandomAccessStream tileData)
        {
            StorageFolder folder = await OpenFolder(tile).ConfigureAwait(false);
            string filename = string.Format(CultureInfo.InvariantCulture, "{0}.tile", tile.CacheKey);
            StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);

            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false))
            {
                Buffer buffer = new Buffer(1024 * 1024);
                bool eof = false;
                while (!eof)
                {
                    IBuffer readBuffer = await tileData.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial).AsTask().ConfigureAwait(false);
                    await fileStream.WriteAsync(readBuffer).AsTask().ConfigureAwait(false);
                    eof = readBuffer.Length < readBuffer.Capacity;
                }
                await fileStream.FlushAsync();
            }
        }
    }
}