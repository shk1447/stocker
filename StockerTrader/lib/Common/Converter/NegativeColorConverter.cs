using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Common.Converter
{
    public class NegativeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double temp = 0;
            if (double.TryParse(System.Convert.ToString(value), out temp))
            {
                if (temp > 0)
                {
                    return "red";
                }
                else if (temp < 0)
                {
                    return "blue";
                }
            }

            return "black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
