using System.Windows;

namespace ModernWPF.Client.Features.Controls
{
    internal class ExtensionProperties : DependencyObject
    {
        public static readonly DependencyProperty AreHandlersSuspended = DependencyProperty.RegisterAttached("AreHandlersSuspended", typeof(bool), typeof(ExtensionProperties), new PropertyMetadata((object)false));

        static ExtensionProperties()
        {
        }

        public static void SetAreHandlersSuspended(DependencyObject obj, bool value)
        {
            obj.SetValue(ExtensionProperties.AreHandlersSuspended, value);
        }

        public static bool GetAreHandlersSuspended(DependencyObject obj)
        {
            return (bool)obj.GetValue(ExtensionProperties.AreHandlersSuspended);
        }
    }
}