using Windows.Foundation;

namespace UniversalMapControl.Interfaces
{
	public interface IProjection
	{
		ILocation ToLocation(Point point, bool sanitize = true);
		Point ToCartesian(ILocation location, bool sanitize = true);
		Point ToCartesian(ILocation location, double referenceLong, bool sanitize = true);
		Point SanitizeCartesian(Point value);

		double GetZoomFactor(double zoomLevel);
		double GetZoomLevel(double zoomFactor);

		Point SanitizeTileIndex(Point index, int zoom);
		Point GetViewPortPositionFromTileIndex(Point tileIndex, int zoom);
		Point GetTileIndex(Point location, int zoom, bool sanitize = true);
		Size CartesianTileSize(ITile tile);
	}
}