using System.Windows.Input;

namespace ModernWPF.Client.Features.Actions
{
    public interface ICommandEx : ICommand
    {
        bool IsExecuting { get; }
    }
}