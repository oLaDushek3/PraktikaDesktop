using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace PraktikaDesktop.Converters
{
    public class TextLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var split = ((string)value).ToCharArray();
            if (split.Length >= 20)
            {
                string result = "";

                for (int i = 0; i <= 20; i++)
                {
                    result += split[i];
                }

                return result + "...";
            }
            else return value;
        }
        public static string Convert(string value)
        {
            var split = (value).ToCharArray();
            if (split.Length >= 20)
            {
                string result = "";

                for (int i = 0; i <= 20; i++)
                {
                    result += split[i];
                }

                return result + "...";
            }
            else return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
