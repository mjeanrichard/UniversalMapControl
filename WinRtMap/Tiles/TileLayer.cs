using System;
using System.Collections.Generic;
using Windows.Foundation;
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
		private const int TileSize = 256;
		private static readonly Wgs84WebMercatorProjection Projection = new Wgs84WebMercatorProjection();


		public TileLayer()
		{
			Tiles = new List<BaseTile>();


			Loaded += TileLayer_Loaded;
		}

		private void TileLayer_Loaded(object sender, RoutedEventArgs e)
		{
			Map parentMap = GetParentMap();
			parentMap.MapCenterChangedEvent += ParentMap_MapCenterChangedEvent;
		}

		protected virtual void RefreshTiles()
		{
			Map parentMap = GetParentMap();
			List<BaseTile> tiles = new List<BaseTile>();

			int zoomLevel = 5;
			int xTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualWidth / TileSize)) + 1;
			int yTileCount = (int)Math.Ceiling(Math.Abs(parentMap.ActualHeight / TileSize)) + 1;

			int tileCount = Math.Max(xTileCount, yTileCount);

			Location mapCenter = parentMap.MapCenter;
			Point centerTileIndex = Projection.GetTileIndex(mapCenter, zoomLevel);
			Point location = Projection.GetViewPortPositionFromTileIndex(centerTileIndex, zoomLevel);
			tiles.Add(new WebTile((int)centerTileIndex.X, (int)centerTileIndex.Y, zoomLevel, location));

			Children.Clear();
			Tiles = tiles;
			foreach (BaseTile tile in Tiles)
			{
				this.Children.Add(tile.Element);
			}
		}

		public List<BaseTile> Tiles { get; protected set; }

		private void ParentMap_MapCenterChangedEvent(object sender, Location e)
		{
			RefreshTiles();
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

			foreach (BaseTile tile in Tiles)
			{
				tile.Element.Arrange(new Rect(tile.Position, new Size(256, 256)));
			}

			return finalSize;
		}
	}

	public interface IHasLocation
	{
		Point Location { get; }
	}
}