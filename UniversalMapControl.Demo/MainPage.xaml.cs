using System;
using System.ComponentModel;

using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using UniversalMapControl.Demo.Models;
using UniversalMapControl.Interfaces;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniversalMapControl.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
	    private readonly DemoModel _viewModel;

	    public MainPage()
        {
            this.InitializeComponent();
	        _viewModel = new DemoModel();
	        DataContext = _viewModel;
			tileLayer.LayerConfiguration.TileLoader.PropertyChanged += TileLoaderOnPropertyChanged;
		}

	    private async void TileLoaderOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
	    {
		    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _pendingTiles.Text = ((ITileLoader)sender).PendingTileCount.ToString());
	    }

	    private void MapOnPointerMoved(object sender, PointerRoutedEventArgs e)
	    {
			PointerPoint mousePoint = e.GetCurrentPoint(map);
			ILocation position = map.GetLocationFromPoint(mousePoint.Position);
			_viewModel.MouseCoordinates = position;
		}
    }
}
