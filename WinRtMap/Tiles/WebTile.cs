using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace WinRtMap.Tiles
{
	public class WebTile : BaseTile
	{
		private readonly Point _location;
		private BitmapImage _bitmap;
		private Image _image;

		public WebTile(int x, int y, int zoom, Point location)
		{
			X = x;
			Y = y;

			Zoom = zoom;
			_location = location;

			Image image = new Image();
			image.IsHitTestVisible = false;
			image.Width = 256;
			image.Height = 256;
			image.Stretch = Stretch.None;
			_bitmap = new BitmapImage();
			image.Source = _bitmap;
			_image = image;
		}

		public bool HasImage { get; protected set; }
		public bool IsRemoved { get; set; }
		public int X { get; protected set; }
		public int Y { get; protected set; }
		public int Zoom { get; protected set; }

		public override UIElement Element
		{
			get { return _image; }
		}

		public override Point Location
		{
			get { return _location; }
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