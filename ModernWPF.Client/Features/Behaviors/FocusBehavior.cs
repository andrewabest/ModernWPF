using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ModernWPF.Client.Features.Behaviors
{
    public class FocusBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += Focus;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= Focus;
        }

        private void Focus(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.Focus();

            var textbox = AssociatedObject as TextBox;
            if (textbox != null)
            {
                textbox.CaretIndex = textbox.Text.Length;
            }
        }
    }
}