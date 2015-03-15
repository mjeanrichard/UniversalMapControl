using System;
using System.IO;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WinRtMap.Projections;
using WinRtMap.Utils;

namespace WinRtMap.Tiles
{
	public abstract class BaseTile
	{
		//public BaseTile(int x, int y)
		//{
		//	Location = new Point(x * 256, y * 256);
		//	Border border = new Border();
		//	border.IsHitTestVisible = false;
		//	border.Width = 256;
		//	border.Height = 256;
		//	border.Background = new SolidColorBrush(Colors.LimeGreen);
		//	border.BorderThickness = new Thickness(1);
		//	border.BorderBrush = new SolidColorBrush(Colors.DarkGreen);

		//	TextBlock textBlock = new TextBlock();
		//	textBlock.Width = 256;
		//	textBlock.Height = 256;
		//	textBlock.Text = Location.X + " / " + Location.Latitude;
		//	textBlock.FontSize = 25;
		//	textBlock.TextAlignment = TextAlignment.Center;
		//	border.Child = textBlock;
		//	_element = border;
		//	MapLayerBase.SetLocation(_element, Location);

		//}

		public abstract UIElement Element { get; }
		public abstract Point Position { get; }
	}

	public class WebTile : BaseTile
	{
		private readonly Point _position;

		private UIElement _image;

		public WebTile(int x, int y, int zoom, Point position)
		{
			X = x;
			Y = y;
			Zoom = zoom;
			_position = position;

			Image image = new Image();
			image.Width = 256;
			image.Height = 256;
			image.Source = new BitmapImage(new Uri(string.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", zoom, x, y)));
			_image = image;
		}

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
	}
}