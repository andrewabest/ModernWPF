using ModernWPF.Client.Features.Controls;

namespace ModernWPF.Client.Features.Application.Customer
{
    public class ConfirmationViewModel : AsyncScreen
    {
        public ConfirmationViewModel()
        {
            DisplayName = "Confirm?";
        }

        public void Confirm()
        {
            TryClose();
        }
    }
}