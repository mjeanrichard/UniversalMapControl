using System;
using System.Threading.Tasks;

using Windows.Storage.Streams;
using Windows.UI.Core;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles
{
	public class CanvasBitmapTile : ICanvasBitmapTile
	{
		private CanvasControl _canvas;

		private CanvasBitmap _cBitmap;
		private Task _task;

		public CanvasBitmapTile(int x, int y, int zoom, ILocation location, string layerName, CanvasControl canvas)
		{
			_canvas = canvas;
			LayerName = layerName;
			X = x;
			Y = y;

			Zoom = zoom;
			Location = location;
		}

		public bool IsDisposed { get; private set; }
		public bool IsCachable { get; set; }

		public int X { get; protected set; }
		public int Y { get; protected set; }
		public int Zoom { get; protected set; }

		public string LayerName { get; protected set; }

		public async Task ReadFromAsync(IRandomAccessStream imageStream)
		{
			if (_canvas == null || IsDisposed)
			{
				return;
			}
			CanvasControl canvas = _canvas;
			if (_cBitmap != null)
			{
				_cBitmap.Dispose();
				_cBitmap = null;
			}
			_cBitmap = await CanvasBitmap.LoadAsync(canvas, imageStream);
			if (!IsDisposed)
			{
				await canvas.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => canvas.Invalidate());
			}
		}

		public ILocation Location { get; }

		public bool HasImage
		{
			get { return _cBitmap != null; }
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
			return _cBitmap;
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
				_canvas = null;
			}
		}
	}
}