using Avalonia.Data.Converters;
using Avalonia.Media;
using PraktikaDesktop.Models;
using System;
using System.Globalization;

namespace PraktikaDesktop.Converters
{
    public class DataGridCellBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Transparent;

            if ((value as SupplyProduct).ListStatus == "new")
                return Brushes.LawnGreen;

            if ((value as SupplyProduct).ListStatus == "removed")
                return Brushes.Red;

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
