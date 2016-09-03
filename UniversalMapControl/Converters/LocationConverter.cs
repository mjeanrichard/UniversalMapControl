using System;

using Windows.UI.Xaml.Data;

using UniversalMapControl.Projections;

namespace UniversalMapControl.Converters
{
    public class LocationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                string[] parts = value.ToString().Split(new[] { ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    double lat;
                    double lon;
                    if (double.TryParse(parts[0], out lat) && double.TryParse(parts[1], out lon))
                    {
                        return new Wgs84Location(lat, lon);
                    }
                }
            }
            return new Wgs84Location();
        }
    }
}