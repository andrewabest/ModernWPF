using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModernWPF.Client.Features.Converters
{
    public class HorizontalAlignmentToTextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var a = value as HorizontalAlignment?;
            if (!a.HasValue) return value;

            switch (a.Value)
            {
                case HorizontalAlignment.Left:
                    return TextAlignment.Left;
                case HorizontalAlignment.Right:
                    return TextAlignment.Right;
                default:
                    return TextAlignment.Center;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}