using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ChatClient.Converters
{
    class ConnectionStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((ClientState)value == ClientState.Disconnected)
                return Brushes.IndianRed;

            else if ((ClientState)value == ClientState.Connecting)
                return Brushes.SandyBrown;

            else
                return Brushes.LightGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
