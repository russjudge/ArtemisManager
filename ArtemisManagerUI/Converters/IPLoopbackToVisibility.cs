using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ArtemisManagerUI.Converters
{
    [ValueConversion(typeof(IPAddress), typeof(Visibility))]
    public class IPLoopbackToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IPAddress address)
            {
                if (parameter is string parm)
                {
                    var parms = parm.Split('|');
                    if (parms.Length > 1)
                    {
                        Visibility IsLoopback = (Visibility)Enum.Parse(typeof(Visibility), parms[0]);
                        Visibility IsNotLoopback = (Visibility)Enum.Parse(typeof(Visibility), parms[1]);
                        if (address.ToString() == IPAddress.Loopback.ToString())
                        {
                            return IsLoopback;
                        }
                        else
                        {
                            return IsNotLoopback;
                        }
                    }
                    else
                    {
                        return default(Visibility);
                    }
                }
                else
                {
                    return default(Visibility);
                }
            }
            else
            {
                return default(Visibility);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
