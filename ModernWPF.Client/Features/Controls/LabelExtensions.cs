using System.Windows;

namespace ModernWPF.Client.Features.Controls
{
    internal static class LabelExtensions
    {
        public static void SetValueNoCallback(this DependencyObject obj, DependencyProperty property, object value)
        {
            ExtensionProperties.SetAreHandlersSuspended(obj, true);
            try
            {
                obj.SetValue(property, value);
            }
            finally
            {
                ExtensionProperties.SetAreHandlersSuspended(obj, false);
            }
        }

        public static bool AreHandlersSuspended(this DependencyObject obj)
        {
            return ExtensionProperties.GetAreHandlersSuspended(obj);
        }
    }
}