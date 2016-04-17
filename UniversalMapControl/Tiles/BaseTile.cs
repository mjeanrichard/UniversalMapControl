using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace UniversalMapControl.Tiles
{
	public abstract class BaseTile : IDisposable
	{
		private CanvasControl _canvas;

		private CanvasBitmap _cBitmap;
		private Task _task;
		private IBuffer _imageBuffer;

		protected BaseTile(int x, int y, int zoom, Location location, string layerName, CanvasControl canvas)
		{
			_canvas = canvas;
			LayerName = layerName;
			X = x;
			Y = y;

			Zoom = zoom;
			Location = location;
		}

		public bool HasImage { get { return _imageBuffer != null; } }
		public bool IsDisposed { get; private set; }
		public int X { get; protected set; }
		public int Y { get; protected set; }
		public int Zoom { get; protected set; }
		public string LayerName { get; protected set; }

		public Location Location { get; }

		private void BitmapOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
		{
		}


		public void SetImage(IRandomAccessStream imageStream)
		{
			if (_canvas == null || IsDisposed)
			{
				return;
			}
			byte[] buffer = new byte[imageStream.Size];
			_imageBuffer = imageStream.ReadAsync(buffer.AsBuffer(), (uint)imageStream.Size, InputStreamOptions.None).AsTask().Result;
			_task = LoadImageAsync();
		}

		private async Task LoadImageAsync()
		{
			CanvasControl canvas = _canvas;
			if (!HasImage || canvas == null)
			{
				return;
			}
			using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
			{
				stream.WriteAsync(_imageBuffer).GetResults();
				stream.Seek(0);
				_cBitmap = await CanvasBitmap.LoadAsync(canvas, stream);
				if (!IsDisposed)
				{
					await canvas.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => canvas.Invalidate());
				}
			}
		}

		public void Reset(CanvasControl canvas)
		{
			_canvas = canvas;
			if (_cBitmap != null)
			{
				_cBitmap.Dispose();
				_cBitmap = null;
			}
			_task = null;
		}

		public CanvasBitmap GetCanvasBitmap()
		{
			if (IsDisposed)
			{
				return null;
			}
			if (_task != null && _task.IsCompleted)
			{
				try
				{
					_task.Wait();
				}
				catch (AggregateException aggregateException)
				{
					if (_cBitmap != null)
					{
						_cBitmap.Dispose();
						_cBitmap = null;
					}
					// .NET async tasks wrap all errors in an AggregateException.
					// We unpack this so Win2D can directly see any lost device errors.
					aggregateException.Handle(exception => { throw exception; });
				}
				finally
				{
					_task = null;
				}
			}
			if (_cBitmap == null && HasImage)
			{
				//_task = LoadImageAsync();
			}
			return _cBitmap;
		}

		public void CopyImage(BaseTile tile)
		{
			if (tile.HasImage)
			{
				_imageBuffer = tile._imageBuffer;
				_task = LoadImageAsync();
			}
		}

		protected virtual void AnimateTile()
		{
			//DoubleAnimation doubleAnimation = new DoubleAnimation {To = 1d, Duration = TimeSpan.FromMilliseconds(500)};
			//doubleAnimation.EasingFunction = new SineEase {EasingMode = EasingMode.EaseInOut};
			//Storyboard.SetTargetProperty(doubleAnimation, "Opacity");
			//Storyboard.SetTarget(doubleAnimation, _image);
			//var storyboard = new Storyboard();
			//storyboard.Children.Add(doubleAnimation);
			//storyboard.Begin();
		}

		public override string ToString()
		{
			return string.Format("{0} / {1}", X, Y);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Protected implementation of Dispose pattern.
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				IsDisposed = true;
				if (_cBitmap != null)
				{
					_cBitmap.Dispose();
					_cBitmap = null;
				}
				_task = null;
				_imageBuffer = null;
				_canvas = null;
			}
		}
	}
}