using System;

using Windows.UI.Xaml.Data;

using UniversalMapControl.Projections;

namespace UniversalMapControl.Converters
{
	public class SwissGridConverter : IValueConverter
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
					int x;
					int y;
					if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
					{
						return new SwissGridLocation(x, y);
					}
				}
			}
			return new Wgs84Location();
		}
	}
}