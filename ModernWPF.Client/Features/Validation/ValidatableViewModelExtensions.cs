namespace ModernWPF.Client.Features.Validation
{
    public static class ValidatableViewModelExtensions
    {
        public static void Initialize(this ValidatableViewModel validatableViewModel, System.Action initializationAction)
        {
            validatableViewModel.IsNotifying = false;

            initializationAction();

            validatableViewModel.SetIsValid();

            validatableViewModel.IsNotifying = true;
        }
    }
}