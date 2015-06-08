using System.Threading.Tasks;
using ModernWPF.Client.Features.Actions;
using ModernWPF.Client.Features.Controls;

namespace ModernWPF.Client.Features.Application.Customer.Actions
{
    public class ShowDialogWithoutResultAction : AsyncActionBase
    {
        private readonly IDialogConductor _dialogConductor;

        public ShowDialogWithoutResultAction(IDialogConductor dialogConductor)
        {
            _dialogConductor = dialogConductor;
        }

        protected override Task ExecuteAsync(object parameter)
        {
            _dialogConductor.ShowDialog<ConfirmationViewModel>();

            return Task.FromResult(0);
        }
    }
}