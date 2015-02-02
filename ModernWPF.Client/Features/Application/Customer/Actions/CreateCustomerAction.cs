using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using ModernWPF.Client.Features.Actions;
using ModernWPF.Client.Features.Alerts;
using ModernWPF.Client.Features.Application.Customer.Messages;

namespace ModernWPF.Client.Features.Application.Customer.Actions
{
    public class CreateCustomerAction : AsyncActionBase, IHandle<CustomerDetailsChangedMessage>
    {
        private CustomerDetailsViewModel _customerDetails;

        public override bool CanExecute
        {
            get { return _customerDetails != null && _customerDetails.IsValid; }
        }

        protected async override Task ExecuteAsync(object parameter)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            Alert.OfSuccess("Customer", "Creation successful!");
        }

        public void Handle(CustomerDetailsChangedMessage message)
        {
            _customerDetails = message.CustomerDetails;

            RaiseCanExecuteChanged();
        }
    }
}