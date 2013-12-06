using Caliburn.Micro;
using ModernWPF.Client.Features.Actions;
using ModernWPF.Client.Features.Alerts;
using ModernWPF.Client.Features.Application.Customer.Messages;

namespace ModernWPF.Client.Features.Application.Customer.Actions
{
    public class CreateCustomerAction : ActionBase, IHandle<CustomerDetailsChangedMessage>
    {
        private CustomerDetailsViewModel _customerDetails;

        public override bool CanExecute
        {
            get { return _customerDetails != null && _customerDetails.IsValid; }
        }

        public override void Execute(object parameter)
        {
            Alert.OfSuccess("Customer", "Creation successful!");
        }

        public void Handle(CustomerDetailsChangedMessage message)
        {
            _customerDetails = message.CustomerDetails;

            RaiseCanExecuteChanged();
        }
    }
}