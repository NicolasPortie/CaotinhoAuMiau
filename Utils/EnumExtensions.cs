using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace CaotinhoAuMiau.Utils
{
    public static class EnumExtensions
    {
        public static string ObterValorMembroEnum<T>(this T enumValue) where T : Enum
        {
            var member = typeof(T).GetMember(enumValue.ToString()).FirstOrDefault();
            var attribute = member?.GetCustomAttribute<EnumMemberAttribute>();
            return attribute?.Value ?? enumValue.ToString();
        }

        public static T ConverterValorMembroEnum<T>(string? value) where T : Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(T)!;
            }

            foreach (var field in typeof(T).GetFields())
            {
                var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attribute != null && string.Equals(attribute.Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return (T)field.GetValue(null)!;
                }
            }

            if (Enum.TryParse(typeof(T), value, ignoreCase: true, out var result))
            {
                return (T)result!;
            }

            return default(T)!;
        }

        public static string GetEnumMemberValue<T>(this T enumValue) where T : Enum
        {
            return ObterValorMembroEnum(enumValue);
        }

        public static T ParseEnumMemberValue<T>(string? value) where T : Enum
        {
            return ConverterValorMembroEnum<T>(value);
        }
    }
}
