using ChatClient.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatClient.Converters
{
    class ConnectionStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((ClientState)value == ClientState.Disconnected)
                return "Disconnected";

            else if ((ClientState)value == ClientState.Connecting)
                return "Connecting";

            else
                return "Connected";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
