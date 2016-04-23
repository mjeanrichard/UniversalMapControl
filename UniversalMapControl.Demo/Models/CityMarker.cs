using Windows.Foundation;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Demo.Models
{
    /// <summary>
    /// Because this model implements the IHasLocation interface the cities are automatically placed at correct 
    /// location when used in a ItemsControl. This is NOT done with a Binding and the location will therefore 
    /// not update on the map if it changes.
    /// </summary>
    public class CityMarker : IHasLocation
    {
        public Location Location { get; set; }
        public string Label { get; set; }
    }
}