using System.Collections.Generic;

namespace ModernWPF.Client.Features.Validation
{
    public interface IValidatable
    {
        bool IsValid { get; set; }

        ICollection<string> ValidationErrors { get; }
    }
}