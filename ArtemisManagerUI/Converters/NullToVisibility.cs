using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ArtemisManagerUI.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class NullToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            Visibility IsNullVisibility = Visibility.Visible;
            Visibility IsObjectVisibility = Visibility.Collapsed;
            if (parameter != null)
            {
                string[] parmSettings = ((string)parameter).Split('|');

                IsNullVisibility = (Visibility)Enum.Parse(typeof(Visibility), parmSettings[0]);
                IsObjectVisibility = (Visibility)Enum.Parse(typeof(Visibility), parmSettings[1]);
            }
            return (value == null || string.IsNullOrEmpty(value.ToString())) ? IsNullVisibility : IsObjectVisibility;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
