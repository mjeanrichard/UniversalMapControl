using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using WinRtMap.Projections;
using WinRtMap.Utils;

namespace WinRtMap.Tiles
{
	public class TileLayer : MapLayerBase
	{
		private TileLoader _tileLoader = new TileLoader();

		public TileLayer()
		{
			Loaded += TileLayer_Loaded;
		}

		private void TileLayer_Loaded(object sender, RoutedEventArgs e)
		{
			Map parentMap = GetParentMap();
			parentMap.ViewPortChangedEvent += ParentMap_ViewPortChangedEvent;
		}

		protected virtual void RefreshTiles()
		{
			Map parentMap = GetParentMap();

			_tileLoader.RefreshTiles(parentMap);


			Children.Clear();
			foreach (BaseTile tile in _tileLoader.GetTiles())
			{
				Children.Add(tile.Element);
			}
		}

		private void ParentMap_ViewPortChangedEvent(object sender, EventArgs e)
		{
			RefreshTiles();
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Map parentMap = GetParentMap();
			RenderTransform = parentMap.ViewPortTransform;

			double viewPortCenterX = parentMap.ViewPortCenter.X;
			double width = (2 << (int)parentMap.ZoomLevel)*64;
			foreach (BaseTile tile in _tileLoader.GetTiles())
			{
				double posX = tile.Position.X;
				if (Math.Abs(viewPortCenterX - posX) > width)
				{
					if (posX < 0)
					{
						posX = posX + width * 2;
					}
					else
					{
						posX = posX - width * 2;
					}
				}
				tile.Element.Arrange(new Rect(posX, tile.Position.Y, 256, 256));
			}

			return finalSize;
		}
	}

	public interface IHasLocation
	{
		Point Location { get; }
	}
}