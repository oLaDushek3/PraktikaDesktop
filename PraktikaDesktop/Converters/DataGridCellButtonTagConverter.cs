using Avalonia.Data.Converters;
using PraktikaDesktop.Models;
using System;
using System.Globalization;

namespace PraktikaDesktop.Converters
{
    public class DataGridCellButtonTagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (value as SupplyProduct).ListStatus == "removed")
                return "ArrowULeftTop";

            return "Trash";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
