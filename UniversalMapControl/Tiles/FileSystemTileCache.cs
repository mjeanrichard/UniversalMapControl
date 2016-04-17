using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace UniversalMapControl.Tiles
{
	public class FileSystemTileCache : ITileCache
    {
        public async Task<IRandomAccessStream> TryGetStream(BaseTile tile)
        {
            StorageFolder folder = await OpenFolder(tile).ConfigureAwait(false);
            string filename = string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}.tile", tile.Zoom, tile.X, tile.Y);
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

        public async Task Add(BaseTile tile, IInputStream imageStream)
        {
            StorageFolder folder = await OpenFolder(tile).ConfigureAwait(false);
            string filename = string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}.tile", tile.Zoom, tile.X, tile.Y);
            StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting).AsTask().ConfigureAwait(false);

            using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite).AsTask().ConfigureAwait(false))
            {
                Buffer buffer = new Buffer(1024 * 1024);
                bool eof = false;
                while (!eof)
                {
                    IBuffer readBuffer = await imageStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial).AsTask().ConfigureAwait(false);
                    await fileStream.WriteAsync(readBuffer).AsTask().ConfigureAwait(false);
                    eof = readBuffer.Length < readBuffer.Capacity;
                }
                await fileStream.FlushAsync();
            }
        }

        private async Task<StorageFolder> OpenFolder(BaseTile tile)
        {
            ApplicationData appData = ApplicationData.Current;
            string folderName = string.Format(CultureInfo.InvariantCulture, "UMCCache\\{0}\\{1}", tile.LayerName, tile.Zoom);
            return await appData.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(false);
        }
    }
}