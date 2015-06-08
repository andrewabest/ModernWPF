using System.Threading.Tasks;
using ModernWPF.Client.Features.Actions;
using ModernWPF.Client.Features.Alerts;
using ModernWPF.Client.Features.Controls;

namespace ModernWPF.Client.Features.Application.Customer.Actions
{
    public class ShowDialogWithResultAction : AsyncActionBase
    {
        private readonly IDialogConductor _dialogConductor;

        public ShowDialogWithResultAction(IDialogConductor dialogConductor)
        {
            _dialogConductor = dialogConductor;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var result = await _dialogConductor.ShowDialog<QuestionViewModel, bool>();

            Alert.OfSuccess("Acceptance Result", result.ToString());
        }
    }
}