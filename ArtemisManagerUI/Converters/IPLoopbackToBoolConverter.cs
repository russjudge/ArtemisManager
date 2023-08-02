using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArtemisManagerUI.Converters
{
    [ValueConversion(typeof(IPAddress), typeof(bool))]
    public class IPLoopbackToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IPAddress client)
            {
                bool retVal = default(bool);
                bool returnIfLoopback = true;
                if (parameter is string parm)
                {
                    if (bool.TryParse(parm, out returnIfLoopback)) 
                    {
                    }
                }
                if (client.ToString() == IPAddress.Loopback.ToString())
                {
                    retVal = returnIfLoopback;
                }
                else
                {
                    retVal = !returnIfLoopback;
                }
                return retVal;
            }
            else
            {
                return default(bool);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
