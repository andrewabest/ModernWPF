using System.ComponentModel;
using Caliburn.Micro;
using ModernWPF.Client.Features.Application.Customer.Actions;
using ModernWPF.Client.Features.Application.Customer.Messages;
using ModernWPF.Client.Features.Controls;

namespace ModernWPF.Client.Features.Application.Customer
{
    public class CustomerViewModel : AsyncScreen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly CreateCustomerAction _createCustomerAction;
        private readonly ShowDialogWithResultAction _showDialogWithResultAction;
        private readonly ShowDialogWithoutResultAction _showDialogWithoutResultAction;

        public CustomerViewModel(
            IEventAggregator eventAggregator, 
            CreateCustomerAction createCustomerAction,
            ShowDialogWithResultAction showDialogWithResultAction,
            ShowDialogWithoutResultAction showDialogWithoutResultAction)
        {
            _eventAggregator = eventAggregator;
            _createCustomerAction = createCustomerAction;
            _showDialogWithResultAction = showDialogWithResultAction;
            _showDialogWithoutResultAction = showDialogWithoutResultAction;
        }

        public CustomerDetailsViewModel CustomerDetails { get; private set; }

        public CreateCustomerAction CreateCustomerAction
        {
            get { return _createCustomerAction; }
        }

        public ShowDialogWithResultAction ShowDialogWithResultAction
        {
            get { return _showDialogWithResultAction; }
        }

        public ShowDialogWithoutResultAction ShowDialogWithoutResultAction
        {
            get { return _showDialogWithoutResultAction; }
        }

        protected override void OnInitialize()
        {
            CustomerDetails = new CustomerDetailsViewModel();
            CustomerDetails.PropertyChanged += CustomerDetailsChanged;
        }

        private void CustomerDetailsChanged(object sender, PropertyChangedEventArgs e)
        {
            _eventAggregator.PublishOnUIThread(new CustomerDetailsChangedMessage(CustomerDetails));
        }
    }
}