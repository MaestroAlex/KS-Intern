using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.Windows.Media;
using System.Diagnostics;

namespace ChatClient.Converter
{
    class TextMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string.IsNullOrEmpty((string)value)) ? new Thickness(0, 8, 0, 8) : new Thickness(0,0,0,5);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
