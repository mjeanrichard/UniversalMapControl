using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl
{
    public class MapItemsPanel : MapLayerBase
    {
        public MapItemsPanel()
        {
            Loaded += ItemsMapLayer_Loaded;
        }

        private void ItemsMapLayer_Loaded(object sender, RoutedEventArgs e)
        {
            Map parentMap = ParentMap;
            parentMap.ViewPortChangedEvent += ParentMap_ViewPortChangedEvent;
            InvalidateArrange();
        }

        private void ParentMap_ViewPortChangedEvent(object sender, EventArgs e)
        {
            InvalidateArrange();
        }

        protected override ILocation GetLocation(UIElement element)
        {
            ContentPresenter contentPresenter = element as ContentPresenter;

            if ((contentPresenter != null) && (contentPresenter.Content != null))
            {
                IHasLocation location = contentPresenter.Content as IHasLocation;
                if (location != null)
                {
                    return location.Location;
                }
            }
            return base.GetLocation(element);
        }
    }
}