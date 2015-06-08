using System;

namespace ModernWPF.Client.Features.Controls
{
    public interface IReturnOnClose : IScreenWithClose
    {
    }

    public interface IReturnOnClose<TResult> : IReturnOnClose
    {
        event EventHandler<TResult> ResultProcessed;
    }
}