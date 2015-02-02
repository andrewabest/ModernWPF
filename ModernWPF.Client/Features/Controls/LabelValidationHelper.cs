using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace ModernWPF.Client.Features.Controls
{
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
}