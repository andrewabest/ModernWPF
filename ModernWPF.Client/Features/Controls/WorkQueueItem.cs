using System;
using System.Threading.Tasks;

namespace ModernWPF.Client.Features.Controls
{
    public class WorkQueueItem
    {
        public Func<Task> TaskFunc { get; private set; }
        public string InitiatorInfo { get; private set; }
        public Action<Task> ContinueWith { get; set; }

        public WorkQueueItem(Func<Task> taskFunc, string initiatorInfo, Action<Task> continueWith = null)
        {
            TaskFunc = taskFunc;
            InitiatorInfo = initiatorInfo;
            ContinueWith = continueWith;
        }
    }
}