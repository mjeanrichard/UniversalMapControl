using Windows.Foundation;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Demo.Models
{
    /// <summary>
    /// Simple model with a Property (PeakLocation) that can be bound to a MapLayer.
    /// </summary>
    public class PeakMarker 
    {
        public ILocation PeakLocation { get; set; }
        public string PeakName { get; set; }
    }
}