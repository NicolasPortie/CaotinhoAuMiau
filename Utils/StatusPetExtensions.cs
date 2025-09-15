using CaotinhoAuMiau.Models.Enums;
using System.Runtime.Serialization;
using System.Reflection;

namespace CaotinhoAuMiau.Utils
{
    public static class StatusPetExtensions
    {
        public static string ObterTexto(this StatusPet status)
        {
            var field = typeof(StatusPet).GetField(status.ToString());
            var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();
            return attribute?.Value ?? status.ToString();
        }

        public static string ObterCssClass(this StatusPet status)
        {
            return status switch
            {
                StatusPet.Disponivel => "status-disponivel",
                StatusPet.EmProcesso => "status-em-processo",
                StatusPet.Adotado => "status-adotado",
                StatusPet.Rascunho => "status-rascunho",
                _ => "status-indefinido"
            };
        }

        public static bool PodeEditar(this StatusPet status)
        {
            return status != StatusPet.Adotado && status != StatusPet.EmProcesso;
        }

        public static bool PodeExcluir(this StatusPet status, bool temAdocaoAtiva = false)
        {
            if (temAdocaoAtiva) return false;
            return status == StatusPet.Rascunho || status == StatusPet.Disponivel;
        }

        public static bool PodeAtivar(this StatusPet status)
        {
            return status == StatusPet.Rascunho;
        }
    }
}