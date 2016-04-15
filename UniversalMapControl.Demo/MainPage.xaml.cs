using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using UniversalMapControl.Demo.Models;

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
		}

	    private void MapOnPointerMoved(object sender, PointerRoutedEventArgs e)
	    {
		    Map map = sender as Map;
		    PointerPoint mousePoint = e.GetCurrentPoint(map);
		    Point position = map.GetLocationFromPoint(mousePoint.Position);
		    _viewModel.MouseCoordinates = position;
	    }
    }
}
