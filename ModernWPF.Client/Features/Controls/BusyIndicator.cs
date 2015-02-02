using System.Windows;
using System.Windows.Controls;

namespace ModernWPF.Client.Features.Controls
{
    public class BusyIndicator : ContentControl
    {
        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy", typeof(bool), typeof(BusyIndicator), new PropertyMetadata(default(bool)));

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public static readonly DependencyProperty ShowOverlayProperty = DependencyProperty.Register(
            "ShowOverlay", typeof(bool), typeof(BusyIndicator), new PropertyMetadata(default(bool), ShowOverlayChanged));

        private static void ShowOverlayChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var indicator = obj as BusyIndicator;
            if (indicator == null) return;
            indicator.OverlayOpacity = indicator.ShowOverlay ? 0.4 : 0;
        }

        public bool ShowOverlay
        {
            get { return (bool)GetValue(ShowOverlayProperty); }
            set { SetValue(ShowOverlayProperty, value); }
        }

        public static readonly DependencyProperty OverlayOpacityProperty = DependencyProperty.Register(
            "OverlayOpacity", typeof(double), typeof(BusyIndicator), new PropertyMetadata(default(double)));

        public double OverlayOpacity
        {
            get { return (double)GetValue(OverlayOpacityProperty); }
            set { SetValue(OverlayOpacityProperty, value); }
        }
    }
}