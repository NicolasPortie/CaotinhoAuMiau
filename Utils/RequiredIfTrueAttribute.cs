using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CaotinhoAuMiau.Utils
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfTrueAttribute : ValidationAttribute
    {
        public string BooleanProperty { get; }

        public RequiredIfTrueAttribute(string booleanProperty)
        {
            BooleanProperty = booleanProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            PropertyInfo? prop = validationContext.ObjectType.GetProperty(BooleanProperty);
            if (prop == null)
            {
                return new ValidationResult($"Propriedade {BooleanProperty} não encontrada.");
            }

            object? propValue = prop.GetValue(validationContext.ObjectInstance, null);
            if (propValue is bool boolVal && boolVal)
            {
                if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
                {
                    return new ValidationResult(ErrorMessage ?? $"O campo {validationContext.DisplayName} é obrigatório.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
