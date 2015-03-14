using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WinRtMap
{
	public class TileLayer : MapLayerBase
	{
		private List<Tile> _tiles;

		public TileLayer()
		{
			_tiles = new List<Tile>();


			Loaded += TileLayer_Loaded;
		}

		private void TileLayer_Loaded(object sender, RoutedEventArgs e)
		{
			Map parentMap = GetParentMap();
			parentMap.MapCenterChangedEvent += ParentMap_MapCenterChangedEvent;
		}

		private void ParentMap_MapCenterChangedEvent(object sender, Point e)
		{
			Map parentMap = GetParentMap();


			List<Tile> tiles = new List<Tile>();
			int xTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualWidth / 256))+1;
			int yTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualHeight / 256))+1;

			int tileCount = Math.Max(xTileCount, yTileCount);

			int xFirstTile = (int)Math.Floor((parentMap.MapCenter.X / 256) + (tileCount / 2));
			int yFirstTile = (int)Math.Floor((parentMap.MapCenter.Y / 256) + (tileCount / 2));

			for (int x = 0; x < tileCount; x++)
			{
				for (int y = 0; y < tileCount; y++)
				{
					tiles.Add(new Tile(xFirstTile - x, yFirstTile - y));
				}
			}

			_tiles = tiles;
			Children.Clear();
			foreach (Tile tile in _tiles)
			{
				this.Children.Add(tile.Element);
			}
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Map parentMap = GetParentMap();

			MatrixTransform transform = RenderTransform as MatrixTransform;
			if (transform == null)
			{
				transform = new MatrixTransform();
				RenderTransform = transform;
			}
			transform.Matrix = parentMap.ViewPortMatrix;
			RenderTransform = transform;

			foreach (Tile tile in _tiles)
			{
				tile.Element.Arrange(new Rect(tile.Location, new Size(256, 256)));
			}

			return finalSize;
		}
	}

	public class Tile
	{
		private UIElement _element;

		public Tile(int x, int y)
		{
			Location = new Point(x * 256, y * 256);
			Border border = new Border();
			border.IsHitTestVisible = false;
			border.Width = 256;
			border.Height = 256;
			border.Background = new SolidColorBrush(Colors.LimeGreen);
			border.BorderThickness = new Thickness(1);
			border.BorderBrush = new SolidColorBrush(Colors.DarkGreen);

			TextBlock textBlock = new TextBlock();
			textBlock.Width = 256;
			textBlock.Height = 256;
			textBlock.Text = Location.X + " / " + Location.Y;
			textBlock.FontSize = 25;
			textBlock.TextAlignment = TextAlignment.Center;
			border.Child = textBlock;
			_element = border;
			MapLayerBase.SetLocation(_element, Location);

		}

		public UIElement Element
		{
			get { return _element; }
		}

		public Point Location { get; set; }
	}

	public interface IHasLocation
	{
		Point Location { get; }
	}
}