using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

namespace UniversalMapControl.Tiles
{
    public abstract class BaseTile
    {
        private readonly Image _image;
        private BitmapImage _bitmap;

        protected BaseTile(int x, int y, int zoom, Location location, string layerName)
        {
            LayerName = layerName;
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
            _bitmap.ImageFailed += BitmapOnImageFailed;
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
        public bool IsCanelled { get; set; }
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public int Zoom { get; protected set; }
        public string LayerName { get; protected set; }

        public UIElement Element
        {
            get { return _image; }
        }

        public Location Location { get; }

        private void BitmapOnImageFailed(object sender, ExceptionRoutedEventArgs exceptionRoutedEventArgs)
        {}

        public void UpdateTransform(double zoomLevel, double angle, Map map)
        {
            double tileScaleFactor = map.ViewPortProjection.GetZoomFactor(zoomLevel - Zoom);
            ScaleTransform.ScaleX = tileScaleFactor;
            ScaleTransform.ScaleY = tileScaleFactor;
            RotateTransform.Angle = angle;
        }

        public async Task SetImage(IRandomAccessStream imageStream)
        {
            await _image.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SetImageSource(imageStream)).AsTask().ConfigureAwait(false);
            HasImage = true;
        }

        public void SetImage(BaseTile tile)
        {
            if (tile.HasImage)
            {
                _bitmap = tile._bitmap;
                _image.Source = _bitmap;
                _image.Opacity = 1;
                HasImage = true;
            }
        }

        private void SetImageSource(IRandomAccessStream imageStream)
        {
            _bitmap.SetSource(imageStream);
            AnimateTile();
        }

        protected virtual void AnimateTile()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation {To = 1d, Duration = TimeSpan.FromMilliseconds(500)};
            doubleAnimation.EasingFunction = new SineEase {EasingMode = EasingMode.EaseInOut};
            Storyboard.SetTargetProperty(doubleAnimation, "Opacity");
            Storyboard.SetTarget(doubleAnimation, _image);
            var storyboard = new Storyboard();
            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin();
        }

        public override string ToString()
        {
            return string.Format("{0} / {1}", X, Y);
        }
    }
}