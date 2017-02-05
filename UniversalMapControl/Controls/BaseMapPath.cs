using System;

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
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ParentMap.ProjectionChanged -= ProjectionChanged;
            ParentMap.ZoomLevelChangedEvent -= ZoomLevelChanged;
            ParentMap = null;
        }

        protected Map ParentMap { get; private set; }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            ParentMap = this.GetParentMap();
            ParentMap.ProjectionChanged += ProjectionChanged;
            ParentMap.ZoomLevelChangedEvent += ZoomLevelChanged;
            Invalidate();
        }

        private void ZoomLevelChanged(object sender, double e)
        {
            Invalidate();
        }

        private void ProjectionChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected virtual void Invalidate()
        {
        }
    }
}