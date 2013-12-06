using Caliburn.Micro;

namespace ModernWPF.Client.Features.Application.Customer
{
    public class CustomerViewModel : Screen
    {
        public CustomerDetailsViewModel CustomerDetails { get; private set; }

        protected override void OnInitialize()
        {
            CustomerDetails = new CustomerDetailsViewModel();
        }
    }
}