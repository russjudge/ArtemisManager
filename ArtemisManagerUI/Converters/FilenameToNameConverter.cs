using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArtemisManagerUI.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    internal class FilenameToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string val)
            { 
                try
                {
                    return new FileInfo(val).Name;
                }
                catch
                {
                    return value;
                }
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
