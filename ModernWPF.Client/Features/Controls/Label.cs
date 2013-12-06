using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
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

    internal class LabelValidationHelper
    {
        internal static readonly DependencyProperty ValidationMetadataProperty = DependencyProperty.RegisterAttached("ValidationMetadata", typeof(LabelValidationMetadata), typeof(LabelValidationHelper), (PropertyMetadata)null);

        static LabelValidationHelper()
        {
        }

        internal static LabelValidationMetadata GetValidationMetadata(DependencyObject inputControl)
        {
            if (inputControl == null)
                throw new ArgumentNullException("inputControl");
            else
                return inputControl.GetValue(LabelValidationHelper.ValidationMetadataProperty) as LabelValidationMetadata;
        }

        internal static void SetValidationMetadata(DependencyObject inputControl, LabelValidationMetadata value)
        {
            if (inputControl == null)
                throw new ArgumentNullException("inputControl");
            inputControl.SetValue(LabelValidationHelper.ValidationMetadataProperty, (object)value);
        }

        internal static LabelValidationMetadata ParseMetadata(FrameworkElement element, bool forceUpdate, out object entity, out BindingExpression bindingExpression)
        {
            entity = (object)null;
            bindingExpression = (BindingExpression)null;
            if (element == null)
                return (LabelValidationMetadata)null;
            if (!forceUpdate)
            {
                LabelValidationMetadata validationMetadata = element.GetValue(LabelValidationHelper.ValidationMetadataProperty) as LabelValidationMetadata;
                if (validationMetadata != null)
                    return validationMetadata;
            }
            foreach (FieldInfo fieldInfo in element.GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
            {
                if (fieldInfo.FieldType == typeof(DependencyProperty))
                {
                    BindingExpression bindingExpression1 = element.GetBindingExpression((DependencyProperty)fieldInfo.GetValue((object)null));
                    if (bindingExpression1 != null && bindingExpression1.ParentBinding != null && bindingExpression1.ParentBinding.Path != null)
                    {
                        entity = bindingExpression1.DataItem ?? element.DataContext;
                        if (entity != null)
                        {
                            if (bindingExpression1.ParentBinding.Mode == BindingMode.TwoWay)
                            {
                                bindingExpression = bindingExpression1;
                                break;
                            }
                            else if (bindingExpression == null || string.Compare(bindingExpression1.ParentBinding.Path.Path, bindingExpression.ParentBinding.Path.Path, StringComparison.Ordinal) < 0)
                                bindingExpression = bindingExpression1;
                        }
                    }
                }
            }

            if (bindingExpression == null)
                return null;

            LabelValidationMetadata validationMetadata1 = ParseMetadata(bindingExpression.ParentBinding.Path.Path, entity);
            element.SetValue(ValidationMetadataProperty, (object)validationMetadata1);
            return validationMetadata1;
        }

        internal static LabelValidationMetadata ParseMetadata(string bindingPath, object entity)
        {
            if (entity != null && !string.IsNullOrEmpty(bindingPath))
            {
                PropertyInfo property = GetProperty(GetCustomOrCLRType(entity), bindingPath);
                if (property != null)
                {
                    var validationMetadata = new LabelValidationMetadata();
                    foreach (object obj in property.GetCustomAttributes(false))
                    {
                        if (obj is RequiredAttribute)
                        {
                            validationMetadata.IsRequired = true;
                        }
                        else
                        {
                            var displayAttribute = obj as DisplayAttribute;
                            if (displayAttribute != null)
                            {
                                validationMetadata.Name = displayAttribute.GetDescription();
                                validationMetadata.Caption = displayAttribute.GetName();
                            }
                        }
                    }
                    if (validationMetadata.Caption == null)
                        validationMetadata.Caption = property.Name;
                    return validationMetadata;
                }
            }
            return (LabelValidationMetadata)null;
        }

        private static PropertyInfo GetProperty(Type entityType, string propertyPath)
        {
            Type type = entityType;
            string[] strArray = propertyPath.Split(new char[1]
      {
        '.'
      });
            if (strArray != null)
            {
                for (int index = 0; index < strArray.Length; ++index)
                {
                    PropertyInfo property = type.GetProperty(strArray[index]);
                    if (property == null || !property.CanRead)
                        return (PropertyInfo)null;
                    if (index == strArray.Length - 1)
                        return property;
                    type = property.PropertyType;
                }
            }
            return (PropertyInfo)null;
        }

        private static Type GetCustomOrCLRType(object instance)
        {
            ICustomTypeProvider icustomTypeProvider = instance as ICustomTypeProvider;
            if (icustomTypeProvider != null)
                return icustomTypeProvider.GetCustomType() ?? instance.GetType();
            else
                return instance.GetType();
        }
    }

    internal class LabelValidationMetadata : INotifyPropertyChanged
    {
        private string _caption;
        private string _name;
        private bool _isRequired;

        public bool IsRequired
        {
            get
            {
                return this._isRequired;
            }
            set
            {
                if (this._isRequired == value)
                    return;
                this._isRequired = value;
                this.NotifyPropertyChanged("IsRequired");
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (!(this._name != value))
                    return;
                this._name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

        public string Caption
        {
            get
            {
                return this._caption;
            }
            set
            {
                if (!(this._caption != value))
                    return;
                this._caption = value;
                this.NotifyPropertyChanged("Caption");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
            if (changedEventHandler == null)
                return;
            changedEventHandler((object)this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal static class LabelExtensions
    {
        public static void SetValueNoCallback(this DependencyObject obj, DependencyProperty property, object value)
        {
            ExtensionProperties.SetAreHandlersSuspended(obj, true);
            try
            {
                obj.SetValue(property, value);
            }
            finally
            {
                ExtensionProperties.SetAreHandlersSuspended(obj, false);
            }
        }

        public static bool AreHandlersSuspended(this DependencyObject obj)
        {
            return ExtensionProperties.GetAreHandlersSuspended(obj);
        }
    }

    internal class ExtensionProperties : DependencyObject
    {
        public static readonly DependencyProperty AreHandlersSuspended = DependencyProperty.RegisterAttached("AreHandlersSuspended", typeof(bool), typeof(ExtensionProperties), new PropertyMetadata((object)false));

        static ExtensionProperties()
        {
        }

        public static void SetAreHandlersSuspended(DependencyObject obj, bool value)
        {
            obj.SetValue(ExtensionProperties.AreHandlersSuspended, value);
        }

        public static bool GetAreHandlersSuspended(DependencyObject obj)
        {
            return (bool)obj.GetValue(ExtensionProperties.AreHandlersSuspended);
        }
    }
}