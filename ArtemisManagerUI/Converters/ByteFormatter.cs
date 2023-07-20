using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArtemisManagerUI.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    internal class ByteFormatter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return default(string);
            }
            else
            {
                long work;
                if (value is long data)
                {
                    work = data;
                }
                else
                {
                    if (value is int data2)
                    {
                        work = data2;
                    }
                    else
                    {
                        return default(string);
                    }
                }
                string[] items = new string[] { "bytes", "KB", "MB", "GB", "TB", "PB", "EB" };
                int p = -1;

                decimal workReturn = decimal.MaxValue;
                //0, 3, 6, 9, 12
                //0 , 1024, 1024 ^2, 
                while (workReturn > 1)
                {
                    p++;
                    workReturn = System.Convert.ToDecimal(work) / System.Convert.ToDecimal(Math.Pow(1024, p));
                    
                }
                if (p > 0)
                {
                    p--;
                    workReturn = System.Convert.ToDecimal(work) / System.Convert.ToDecimal(Math.Pow(1024, p));
                }
                return Math.Round(workReturn, 1).ToString() + items[p];
            }
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
