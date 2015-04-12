using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace WinRtMap.Tiles
{
	public abstract class BaseTile
	{
		private BitmapImage _bitmap;
		private Image _image;

		protected BaseTile(int x, int y, int zoom, Point location)
		{
			X = x;
			Y = y;

			Zoom = zoom;
			Location = location;

			Image image = new Image();
			image.IsHitTestVisible = false;
			image.Width = 256;
			image.Opacity = 0;
			image.Height = 256;
			image.Stretch = Stretch.None;
			_bitmap = new BitmapImage();
			image.Source = _bitmap;
			_image = image;

			TileTransform = new TransformGroup();
			RotateTransform = new RotateTransform();
			ScaleTransform = new ScaleTransform();
			TileTransform.Children.Add(ScaleTransform);
			TileTransform.Children.Add(RotateTransform);
			_image.RenderTransform = TileTransform;
		}

		protected TransformGroup TileTransform { get; }
		protected RotateTransform RotateTransform { get; }
		protected ScaleTransform ScaleTransform { get; }
		public bool HasImage { get; protected set; }
		public bool IsRemoved { get; set; }
		public int X { get; protected set; }
		public int Y { get; protected set; }
		public int Zoom { get; protected set; }

		public UIElement Element
		{
			get { return _image; }
		}

		public Point Location { get; }

		public void UpdateTransform(double zoomLevel, double angle, Map map)
		{
			double tileScaleFactor = map.GetScaleFactor(zoomLevel - Zoom);
			ScaleTransform.ScaleX = tileScaleFactor;
			ScaleTransform.ScaleY = tileScaleFactor;
			RotateTransform.Angle = angle;
		}

		public async Task SetImage(IRandomAccessStream imageStream)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetImageSource(imageStream));
			HasImage = true;
		}

		private void SetImageSource(IRandomAccessStream imageStream)
		{
			_bitmap.SetSource(imageStream);
			AnimateTile();
		}

		protected virtual void AnimateTile()
		{
			DoubleAnimation doubleAnimation = new DoubleAnimation {To = 1d, Duration = TimeSpan.FromMilliseconds(500)};
			doubleAnimation.EasingFunction = new CubicEase();
			Storyboard.SetTargetProperty(doubleAnimation, "Opacity");
			Storyboard.SetTarget(doubleAnimation, _image);
			var storyboard = new Storyboard();
			storyboard.Children.Add(doubleAnimation);
			storyboard.Begin();
		}
	}
}