using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ArtemisEngineeringPresets
{
    [ValueConversion(typeof(int), typeof(Brush))]
    public class GreaterThanBrushConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Brush? retVal = null;
            if (value is int val && parameter is string parm)
            {
                string[] parms = parm.Split('|');
                Brush? brushIfMatch = null;
                Brush? brushIfNoMatch = null;
                if (int.TryParse(parms[0], out int match))
                {
                    if (parms.Length > 1)
                    {
                        BrushConverter brush = new();
                        brushIfMatch = brush.ConvertFromInvariantString(parms[1]) as Brush;
                        if (parms.Length > 2)
                        {
                            brushIfNoMatch = brush.ConvertFromInvariantString(parms[2]) as Brush;
                        }
                    }
                    retVal = (val > match) ? brushIfMatch : brushIfNoMatch;
                }
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
