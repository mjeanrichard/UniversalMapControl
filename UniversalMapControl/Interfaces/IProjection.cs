namespace UniversalMapControl.Interfaces
{
    public interface IProjection
    {
        ILocation ToLocation(CartesianPoint point, bool sanitize = true);
        CartesianPoint ToCartesian(ILocation location, bool sanitize = true);
        CartesianPoint ToCartesian(ILocation location, double referenceLong, bool sanitize = true);
        CartesianPoint SanitizeCartesian(CartesianPoint value);

        double GetZoomFactor(double zoomLevel);

        /// <summary>
        /// This ZoomLevel implementation is based on the Zoomlevels use in online maps. If the zoomlevel is increased by 1 the scale factor doubles.
        /// </summary>
        double GetZoomLevel(double zoomFactor);

        /// <summary>
        /// Get the cartesian scale factor (Cartesian Units per Meter) at the given location. 
        /// This factor can be used to scale a distance in meters to the according view port length.
        /// </summary>
        double CartesianScaleFactor(ILocation center);
    }
}