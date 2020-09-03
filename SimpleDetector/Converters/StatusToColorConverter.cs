using SimpleDetector.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SimpleDetector.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ErroCode code = (ErroCode)value;
            string color;
            switch(code)
            {
                case ErroCode.Ok:
                    color = "#42ab2a";
                    break;
                case ErroCode.ControlBit:
                    color = "#4563CC";
                    break;
                case ErroCode.ControllBitError:
                    color = "#8B0000";
                        break;
                case ErroCode.ErrorDetected:
                    color = "#d48535";
                    break;
                case ErroCode.ErrorNotDetected:
                    color = "#D63220";
                    break;
                case ErroCode.ErrorFixed:
                    color = "#294F43";
                    break;
                case ErroCode.ControlBitFixed:
                    color = "#242D72";
                    break;
                default:
                    return null;
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
