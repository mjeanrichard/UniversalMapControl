using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;

using UniversalMapControl.Utils;

namespace UniversalMapControl.Controls
{
    public abstract class BaseMapPath : Path
    {
        public BaseMapPath()
        {
            Loaded += OnLoaded;
        }

        protected Map ParentMap { get; private set; }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            ParentMap = this.GetParentMap();
        }
    }
}