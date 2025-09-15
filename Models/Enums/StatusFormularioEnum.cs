using System.Runtime.Serialization;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Models.Enums
{
    public enum StatusFormulario
    {
        [EnumMember(Value = "Pendente")]
        Pendente = 1,

        [EnumMember(Value = "Em Análise")]
        EmAnalise = 2,

        [EnumMember(Value = "Aprovado")]
        Aprovado = 3,

        [EnumMember(Value = "Negado")]
        Negado = 4,

        [EnumMember(Value = "Cancelado pelo Usuário")]
        CanceladoPeloUsuario = 5,

        [EnumMember(Value = "Cancelado - Inatividade")]
        CanceladoPorInatividade = 6
    }

    public static class StatusFormularioExtensions
    {
        public static string ObterTexto(this StatusFormulario status)
        {
            return status.ObterValorMembroEnum();
        }

        public static string ObterCssClass(this StatusFormulario status)
        {
            return status switch
            {
                StatusFormulario.Pendente => "pendente",
                StatusFormulario.EmAnalise => "em-analise",
                StatusFormulario.Aprovado => "aprovado",
                StatusFormulario.Negado => "negado",
                StatusFormulario.CanceladoPeloUsuario => "cancelado-usuario",
                StatusFormulario.CanceladoPorInatividade => "cancelado-inatividade",
                _ => "desconhecido"
            };
        }

        public static string ObterIcone(this StatusFormulario status)
        {
            return status switch
            {
                StatusFormulario.Pendente => "fas fa-hourglass-half",
                StatusFormulario.EmAnalise => "fas fa-search",
                StatusFormulario.Aprovado => "fas fa-check-circle",
                StatusFormulario.Negado => "fas fa-times-circle",
                StatusFormulario.CanceladoPeloUsuario => "fas fa-user-times",
                StatusFormulario.CanceladoPorInatividade => "fas fa-clock",
                _ => "fas fa-question-circle"
            };
        }

        public static bool PodeEditar(this StatusFormulario status)
        {
            return status == StatusFormulario.Pendente;
        }

        public static bool PodeAvaliar(this StatusFormulario status)
        {
            return status == StatusFormulario.Pendente || 
                   status == StatusFormulario.EmAnalise;
        }

        public static bool PodeCancelar(this StatusFormulario status)
        {
            return status == StatusFormulario.Pendente || 
                   status == StatusFormulario.EmAnalise;
        }

        public static bool EstaAtivo(this StatusFormulario status)
        {
            return status == StatusFormulario.Pendente || 
                   status == StatusFormulario.EmAnalise;
        }

        public static bool FoiAprovado(this StatusFormulario status)
        {
            return status == StatusFormulario.Aprovado;
        }

        public static bool FoiNegado(this StatusFormulario status)
        {
            return status == StatusFormulario.Negado;
        }

        public static bool FoiCancelado(this StatusFormulario status)
        {
            return status == StatusFormulario.CanceladoPeloUsuario || 
                   status == StatusFormulario.CanceladoPorInatividade;
        }
    }
}