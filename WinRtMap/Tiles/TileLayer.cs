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
		private static readonly Wgs84WebMercatorProjection Projection = new Wgs84WebMercatorProjection();
		private TileLoader _tileLoader = new TileLoader();

		public TileLayer()
		{
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

			_tileLoader.RefreshTiles(parentMap);


			Children.Clear();
			foreach (BaseTile tile in _tileLoader.GetTiles())
			{
				Children.Add(tile.Element);
			}
		}

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

			foreach (BaseTile tile in _tileLoader.GetTiles())
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