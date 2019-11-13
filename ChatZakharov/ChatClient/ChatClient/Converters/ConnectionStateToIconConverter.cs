using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ChatClient.Converters
{
    class ConnectionStateToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((ClientState)value == ClientState.Disconnected)
                return "CloseCircle";

            else if ((ClientState)value == ClientState.Connecting)
                return "DotsHorizontalCircle";

            else
                return "CheckCircle";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
