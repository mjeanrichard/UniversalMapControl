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

		ILocation ChangeLatitude(double newLatitude);
		ILocation ChangeLongitude(double newLongitude);
	}
}