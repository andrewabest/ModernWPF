using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ModernWPF.Client.Extensions;

namespace ModernWPF.Client.Features.Validation
{
    /// <summary>
    /// This class is used by https://github.com/Fody/Validar to inject validation into our viewmodels.
    /// </summary>
    // ReSharper disable UnusedMember.Global
    public class ValidationTemplate<T> : INotifyDataErrorInfo where T : IValidatable
    // ReSharper restore UnusedMember.Global
    {
        private readonly INotifyPropertyChanged _target;
        private readonly ValidationContext _validationContext;
        private readonly List<ValidationResult> _validationResults;

        public ValidationTemplate(INotifyPropertyChanged target)
        {
            _target = target;
            _validationContext = new ValidationContext(target, null, null);
            _validationResults = new List<ValidationResult>();
            target.PropertyChanged += (s, e) => Validate((T)s, e);
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void Validate(T sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid" || e.PropertyName == "EventContext" || e.PropertyName == "ContactContext" || e.PropertyName == "IsDirty") return;

            _validationResults.Clear();

            Validator.TryValidateObject(_target, _validationContext, _validationResults, true);

            var hashSet = new HashSet<string>(_validationResults.SelectMany(x => x.MemberNames));

            sender.IsValid = hashSet.None();

            foreach (var error in hashSet)
            {
                RaiseErrorsChanged(error);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return _validationResults.Where(x => x.MemberNames.Contains(propertyName))
                                    .Select(x => x.ErrorMessage);
        }

        public bool HasErrors
        {
            get { return _validationResults.Count > 0; }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            var handler = ErrorsChanged;
            if (handler != null)
            {
                handler(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
    }
}