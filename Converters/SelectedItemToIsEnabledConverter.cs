using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Wallanguager.Converters
{
	public class SelectedItemToIsEnabledConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// ReSharper disable once PossibleNullReferenceException
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return false;
		}
	}
}
