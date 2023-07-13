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
    [ValueConversion(typeof(DateTime), typeof(string))]
    internal class DateFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                {
                    return default(DateTime);
                }
                else
                {
                    if (parameter is not string parm)
                    {
                        return value;
                    }
                    else
                    {
                        return ((DateTime)value).ToString(parm);
                    }
                }
            }
            catch (Exception ex)
            {
                return default(DateTime);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
