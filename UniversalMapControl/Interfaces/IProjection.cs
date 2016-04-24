using Windows.Foundation;

namespace UniversalMapControl.Interfaces
{
	public interface IProjection
	{
		ILocation ToLocation(Point point, bool sanitize = true);
		Point ToCartesian(ILocation location, bool sanitize = true);
		Point ToCartesian(ILocation location, double referenceLong, bool sanitize = true);

		double GetZoomFactor(double zoomLevel);
		Point SanitizeCartesian(Point value);
		double GetZoomLevel(double zoomFactor);
	}
}