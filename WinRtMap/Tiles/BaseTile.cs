using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WinRtMap.Utils;

namespace WinRtMap.Tiles
{
	public abstract class BaseTile
	{
		public abstract UIElement Element { get; }
		public abstract Point Position { get; }
	}

	public class TileLoader
	{
		private const int TileSize = 256;
		private HttpClient _client = new HttpClient();
		private Dictionary<string, WebTile> _tiles = new Dictionary<string, WebTile>();
		private ConcurrentQueue<WebTile> _tilesToLoad = new ConcurrentQueue<WebTile>();

		public void RefreshTiles(Map parentMap)
		{
			Dictionary<string, WebTile> newTiles = new Dictionary<string, WebTile>();

			int xTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualWidth / TileSize)) + 1;
			int yTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualHeight / TileSize)) + 1;

			int tileCount = (int)(Math.Ceiling(Math.Max(xTileCount, yTileCount) / 2d) + 1);

			Location mapCenter = parentMap.MapCenter;
			Point centerTileIndex = parentMap.ViewPortProjection.GetTileIndex(mapCenter, (int)parentMap.ZoomLevel);


			WebTile tile;
			while (_tilesToLoad.TryDequeue(out tile))
			{ }

			for (int dx = -tileCount; dx < tileCount; dx++)
			{
				for (int dy = -tileCount; dy < tileCount; dy++)
				{
                    int x = parentMap.ViewPortProjection.SanitizeIndex((int)(centerTileIndex.X) + dx, (int)parentMap.ZoomLevel);
					int y = parentMap.ViewPortProjection.SanitizeIndex((int)(centerTileIndex.Y) + dy, (int)parentMap.ZoomLevel);

					string key = string.Join("/", x, y, (int)parentMap.ZoomLevel);

					if (newTiles.ContainsKey(key))
					{
						continue;
					}
					if (_tiles.TryGetValue(key, out tile))
					{
						newTiles.Add(key, tile);
						Enqueue(tile);
					}
					else
					{
						Point location = parentMap.ViewPortProjection.GetViewPortPositionFromTileIndex(new Point(x, y), (int)parentMap.ZoomLevel);
						tile = new WebTile(x, y, (int)parentMap.ZoomLevel, location);
						Enqueue(tile);
						newTiles.Add(key, tile);
					}
				}
			}
			_tiles = newTiles;
		}

		protected void Enqueue(WebTile tile)
		{
			if (!tile.HasImage)
			{
				_tilesToLoad.Enqueue(tile);
				StartDownloading();
			}
		}


		private volatile int _taskCount = 0;
		 

		private void StartDownloading()
		{
			if (_taskCount >= 5)
			{
				return;
			}
			Interlocked.Increment(ref _taskCount);
			Debug.WriteLine(_taskCount);

			Task.Run(async () =>
			{
				WebTile tile;
				while (_tilesToLoad.TryDequeue(out tile))
				{
					if (tile.HasImage)
					{
						continue;
					}
					try
					{
						Uri uri = tile.Uri;
						Debug.WriteLine("Downloading Tile " + tile.X + "/" + tile.Y);

						using (HttpResponseMessage response = await _client.GetAsync(uri))
						{
							using (MemoryStream memStream = new MemoryStream())
							{
								await response.Content.CopyToAsync(memStream);
								memStream.Position = 0;
								using (IRandomAccessStream ras = memStream.AsRandomAccessStream())
								{
									await tile.SetImage(ras);
								}
							}
						}
					}
					catch (Exception e)
					{}
				}
				Interlocked.Decrement(ref _taskCount);
			});
		}

		public IEnumerable<BaseTile> GetTiles()
		{
			return _tiles.Values;
		}
	}

	public class WebTile : BaseTile
	{
		private readonly Point _position;
		private BitmapImage _bitmap;
		private Image _image;

		public WebTile(int x, int y, int zoom, Point position)
		{
			X = x;
			Y = y;

			Zoom = zoom;
			_position = position;

			Image image = new Image();
			image.Width = 256;
			image.Height = 256;
			image.IsHitTestVisible = false;
			image.Stretch = Stretch.None;
			_bitmap = new BitmapImage();
			image.Source = _bitmap;
			_image = image;
		}

		public bool HasImage { get; protected set; }
		public int X { get; protected set; }
		public int Y { get; protected set; }
		public int Zoom { get; protected set; }

		public override UIElement Element
		{
			get { return _image; }
		}

		public override Point Position
		{
			get { return _position; }
		}

		public Uri Uri
		{
			get { return new Uri(string.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", Zoom, X, Y)); }
		}

		public async Task SetImage(IRandomAccessStream imageStream)
		{
			await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { _bitmap.SetSource(imageStream); });
			HasImage = true;
		}
	}
}