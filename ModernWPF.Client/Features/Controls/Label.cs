using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWPF.Client.Features.Controls
{
    /// <summary>
    ///     Displays a caption, required field indicator, and validation error indicator for an associated control.
    /// </summary>
    public class Label : ContentControl
    {
        private new static readonly DependencyProperty DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), typeof(Label), new PropertyMetadata(OnDataContextPropertyChanged));

        public static readonly DependencyProperty IsRequiredProperty = DependencyProperty.Register("IsRequired", typeof(bool), typeof(Label), new PropertyMetadata(OnIsRequiredPropertyChanged));

        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register("IsValid", typeof(bool), typeof(Label), new PropertyMetadata(true, OnIsValidPropertyChanged));

        public static readonly DependencyProperty PropertyPathProperty = DependencyProperty.Register("PropertyPath", typeof(string), typeof(Label), new PropertyMetadata(OnPropertyPathPropertyChanged));

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(Label), new PropertyMetadata(OnTargetPropertyChanged));

        private readonly List<ValidationError> _errors;

        private bool _canContentUseMetaData;
        private bool _initialized;
        private bool _isContentBeingSetInternally;
        private bool _isRequiredOverridden;
        private bool _requiredSetByMetadata;
        private bool _requiredHasBeenSet;

        static Label()
        {
        }

        public Label()
        {
            DefaultStyleKey = typeof(Label);
            _errors = new List<ValidationError>();
            SetBinding(DataContextProperty, new Binding());
            Loaded += LabelLoaded;
            _canContentUseMetaData = Content == null;
        }

        public bool IsRequired
        {
            get { return (bool)GetValue(IsRequiredProperty); }
            set
            {
                _isRequiredOverridden = true;
                SetValue(IsRequiredProperty, value);
            }
        }

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            private set { (this).SetValueNoCallback(IsValidProperty, value); }
        }

        public string PropertyPath
        {
            get { return GetValue(PropertyPathProperty) as string; }
            set { SetValue(PropertyPathProperty, value); }
        }

        public FrameworkElement Target
        {
            get { return GetValue(TargetProperty) as FrameworkElement; }
            set { SetValue(TargetProperty, value); }
        }

        internal LabelValidationMetadata ValidationMetadata { get; set; }

        internal new bool Initialized
        {
            get { return _initialized; }
        }

        private static void OnDataContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var label = d as Label;
            if (label == null || e.OldValue != null && e.NewValue != null && e.OldValue.GetType() == e.NewValue.GetType())
                return;
            label.LoadMetadata(false);
        }

        private static void OnIsRequiredPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var label = d as Label;
            if (label == null || (label).AreHandlersSuspended())
                return;
            label._requiredHasBeenSet = true;
        }

        private static void OnIsValidPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var label = d as Label;
            if (label == null || (label).AreHandlersSuspended())
                return;
            (label).SetValueNoCallback(IsValidProperty, e.OldValue);
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Underlying property is readonly", new object[1]
                                                                                                                                 {
                                                                                                                                     "IsValid"
                                                                                                                                 }));
        }

        private static void OnPropertyPathPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var label = depObj as Label;
            if (label == null || !label.Initialized)
                return;
            label.LoadMetadata(false);
            label.ParseTargetValidState();
        }

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var label = d as Label;
            if (label == null)
                return;
            label.LoadMetadata(false);
            label._errors.Clear();
            var frameworkElement1 = e.OldValue as FrameworkElement;
            var frameworkElement2 = e.NewValue as FrameworkElement;
            EventHandler<ValidationErrorEventArgs> eventHandler = label.TargetBindingValidationError;
            if (frameworkElement1 != null)
                System.Windows.Controls.Validation.RemoveErrorHandler(frameworkElement1, eventHandler);
            if (frameworkElement2 != null)
            {
                System.Windows.Controls.Validation.AddErrorHandler(frameworkElement2, eventHandler);
                ReadOnlyObservableCollection<ValidationError> errors = System.Windows.Controls.Validation.GetErrors(frameworkElement2);
                if (errors.Count > 0)
                    label._errors.Add(errors[0]);
            }
            label.ParseTargetValidState();
        }

        public virtual void Refresh()
        {
            _isRequiredOverridden = false;
            LoadMetadata(true);
            ParseTargetValidState();
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            _canContentUseMetaData = _isContentBeingSetInternally || newContent == null;
        }

        private void SetContentInternally(object value)
        {
            try
            {
                _isContentBeingSetInternally = true;
                Content = value;
            }
            finally
            {
                _isContentBeingSetInternally = false;
            }
        }

        private void LabelLoaded(object sender, RoutedEventArgs e)
        {
            if (_initialized)
                return;
            LoadMetadata(false);
            _initialized = true;
            Loaded -= LabelLoaded;
        }

        private void LoadMetadata(bool forceUpdate)
        {
            LabelValidationMetadata validationMetadata = null;
            object entity;
            BindingExpression bindingExpression;

            if (!string.IsNullOrEmpty(PropertyPath))
                validationMetadata = LabelValidationHelper.ParseMetadata(PropertyPath, DataContext);
            else if (Target != null)
                validationMetadata = LabelValidationHelper.ParseMetadata(Target, forceUpdate, out entity, out bindingExpression);
            if (ValidationMetadata == validationMetadata)
                return;

            ValidationMetadata = validationMetadata;
            if (ValidationMetadata != null)
            {
                string caption = ValidationMetadata.Caption;
                if (caption != null && _canContentUseMetaData)
                    SetContentInternally(caption);
            }
            else if (_canContentUseMetaData)
                SetContentInternally(null);
            if (_isRequiredOverridden || (_requiredHasBeenSet && !_requiredSetByMetadata))
                return;
            bool flag = ValidationMetadata != null && ValidationMetadata.IsRequired;
            _requiredSetByMetadata = true;
            SetValue(IsRequiredProperty, flag);
        }

        private void ParseTargetValidState()
        {
            IsValid = _errors.Count == 0;
        }

        private void TargetBindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
            {
                if (!_errors.Contains(e.Error))
                    _errors.Add(e.Error);
            }
            else if (e.Action == ValidationErrorEventAction.Removed && _errors.Contains(e.Error))
                _errors.Remove(e.Error);
            ParseTargetValidState();
        }
    }
}