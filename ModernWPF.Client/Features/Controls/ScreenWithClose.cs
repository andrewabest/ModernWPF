using Caliburn.Micro;

namespace ModernWPF.Client.Features.Controls
{
    public interface IScreenWithClose : IScreen
    {
        bool HasClose { get; }
    }

    public abstract class ScreenWithClose : Screen, IScreenWithClose
    {
        public virtual bool HasClose
        {
            get { return true; }
        }
    }
}