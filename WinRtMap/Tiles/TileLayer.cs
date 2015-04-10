using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

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

			TransformGroup tileTransform = new TransformGroup();
			// The tiles need to be scaled according to ZoomLevel. The integer part is already taken care of by the image.
			// TODO: It might be better to sacle images down instead of up. I.e. use the image with the higher zoomlevel instead
			double tileScaleFactor = parentMap.GetScaleFactor(parentMap.ZoomLevel % 1);
			tileTransform.Children.Add(new ScaleTransform {ScaleY = tileScaleFactor, ScaleX = tileScaleFactor});
			tileTransform.Children.Add(new RotateTransform {Angle = parentMap.Heading});

			double centerLongitude = parentMap.MapCenter.X;
			foreach (BaseTile tile in _tileLoader.GetTiles())
			{
				Point position = parentMap.ViewPortProjection.ToCartesian(tile.Location, centerLongitude);
				Point tileOrigin = parentMap.ViewPortTransform.TransformPoint(position);
				tile.Element.Arrange(new Rect(tileOrigin.X, tileOrigin.Y, 256, 256));
				tile.Element.RenderTransform = tileTransform;
			}

			return finalSize;
		}
	}

	public interface IHasLocation
	{
		Point Location { get; }
	}
}