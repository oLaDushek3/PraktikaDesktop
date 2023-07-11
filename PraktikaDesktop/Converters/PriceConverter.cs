using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace PraktikaDesktop.Converters
{
    public class PriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                decimal result = (decimal)value;
                return Math.Round(result, 2) + "₽";
            }
            else
                return "См. категории";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
