namespace UniversalMapControl.Behaviors
{
	public class TouchMapEventArgs
	{
		public double Heading { get; set; }
		public double ZoomLevel { get; set; }
		public CartesianPoint ViewPortCenter { get; set; }
	}
}