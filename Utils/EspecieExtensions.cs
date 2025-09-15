using CaotinhoAuMiau.Models.Enums;
using System.ComponentModel;
using System.Reflection;

namespace CaotinhoAuMiau.Utils
{
    public static class EspecieExtensions
    {
        public static string ObterTexto(this Especie especie)
        {
            var field = typeof(Especie).GetField(especie.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? especie.ToString();
        }

        public static string ObterTermoAmigavel(this Especie especie)
        {
            return especie switch
            {
                Especie.Cao => "cachorrinho",
                Especie.Felino => "gatinho",
                _ => especie.ObterTexto().ToLower()
            };
        }
    }
}