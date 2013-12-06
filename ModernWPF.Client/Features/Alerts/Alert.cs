using Caliburn.Micro;

namespace ModernWPF.Client.Features.Alerts
{
    public static class Alert
    {
        public static IEventAggregator EventAggregator { get; set; }

        public static void OfInformation(string title, string message)
        {
            EventAggregator.Publish(new AlertMessage(title, message, AlertSeverity.Information));
        }

        public static void OfWarning(string title, string message)
        {
            EventAggregator.Publish(new AlertMessage(title, message, AlertSeverity.Warning));
        }

        public static void OfSuccess(string title, string message)
        {
            EventAggregator.Publish(new AlertMessage(title, message, AlertSeverity.Success));
        }

        public static void OfError(string title, string message)
        {
            EventAggregator.Publish(new AlertMessage(title, message, AlertSeverity.Error));
        }
    }
}