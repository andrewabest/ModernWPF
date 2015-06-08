using System;
using ModernWPF.Client.Features.Controls;

namespace ModernWPF.Client.Features.Application.Customer
{
    public class QuestionViewModel : AsyncScreen, IReturnOnClose<bool>
    {
        public event EventHandler<bool> ResultProcessed;

        public QuestionViewModel()
        {
            DisplayName = "Do You Accept?";
        }

        public bool IsAccepted { get; set; }

        public void Accept()
        {
            var handler = ResultProcessed;
            if (handler != null)
            {
                handler(this, IsAccepted);
            }
        }
    }
}