namespace ModernWPF.Client.Features.Alerts
{
    public class AlertMessage
    {
        public AlertModel Alert { get; private set; }

        public AlertMessage(string title, string message, AlertSeverity severity)
        {
            Alert = new AlertModel(title, message, severity);
        }
    }
}