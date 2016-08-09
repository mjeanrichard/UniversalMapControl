using Windows.Foundation.Metadata;

namespace UniversalMapControl.Interfaces
{
	public interface ILocation
	{
		/// <summary>
		/// Latitude, Y-Axis or North-South value, depending on the used Projection
		/// </summary>
		double Latitude { get; }

		/// <summary>
		/// Longitude, X-Axis or East-West value, depending on the used Projection
		/// </summary>
		double Longitude { get; }

		[Deprecated("Do not use.", DeprecationType.Deprecate, 1)]
		ILocation ChangeLatitude(double newLatitude);

		[Deprecated("Do not use.", DeprecationType.Deprecate, 1)]
		ILocation ChangeLongitude(double newLongitude);

		string ToString(string format);
		double DistanceTo(ILocation to);
	}
}