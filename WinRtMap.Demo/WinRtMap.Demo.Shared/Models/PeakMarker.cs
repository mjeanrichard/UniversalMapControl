using Windows.Foundation;

namespace WinRtMap.Demo.Models
{
    /// <summary>
    /// Simple model with a Property (PeakLocation) that can be bound to a MapLayer.
    /// </summary>
    public class PeakMarker 
    {
        public Point PeakLocation { get; set; }
        public string PeakName { get; set; }
    }
}