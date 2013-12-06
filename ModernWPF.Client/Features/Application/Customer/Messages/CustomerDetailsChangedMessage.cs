namespace ModernWPF.Client.Features.Application.Customer.Messages
{
    public class CustomerDetailsChangedMessage
    {
        public CustomerDetailsChangedMessage(CustomerDetailsViewModel customerDetails)
        {
            CustomerDetails = customerDetails;
        }

        public CustomerDetailsViewModel CustomerDetails { get; private set; }
    }
}