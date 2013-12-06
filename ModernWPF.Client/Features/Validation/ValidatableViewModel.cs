using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Caliburn.Micro;
using ModernWPF.Client.Extensions;
using Validar;

namespace ModernWPF.Client.Features.Validation
{
    [InjectValidation]
    public class ValidatableViewModel : PropertyChangedBase, IValidatable
    {
        private readonly ICollection<string> _validationErrors;

        protected ValidatableViewModel()
        {
            _validationErrors = new List<string>();

            SetIsValid();

            OnInitialize();
        }

        public bool IsValid { get; set; }

        public bool IsDirty { get; set; }

        public ICollection<string> ValidationErrors
        {
            get { return _validationErrors; }
        }

        protected virtual void Initialize()
        {
        }

        private void OnInitialize()
        {
            IsNotifying = false;

            Initialize();

            IsNotifying = true;
        }

        /// <summary>
        /// This is so we can establish a VVMs initial IsValid state without having it reflect directly on the UI.
        /// </summary>
        internal void SetIsValid()
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, new ValidationContext(this, null, null), results, true);
            IsValid = results.None();
        }
    }
}