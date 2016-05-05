using Windows.Devices.Bluetooth.Advertisement;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles
{
	public interface ILayerConfiguration
	{
		ITileProvider TileProvider { get; }
		string LayerName { get; }
	}
}