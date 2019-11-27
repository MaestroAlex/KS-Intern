using ChatClient.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TransitPackage;

namespace ChatClient.Converters
{
    class ChannelTypeToPasswordBoxVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((ChannelType)value == ChannelType.public_closed)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
