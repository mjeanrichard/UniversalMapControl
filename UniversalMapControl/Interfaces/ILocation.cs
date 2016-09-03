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

        /// <summary>
        /// Returns the distance in meters to the provided location.
        /// </summary>
        double DistanceTo(ILocation to);

        string ToString(string format);
    }
}