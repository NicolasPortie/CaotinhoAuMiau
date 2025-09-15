using CaotinhoAuMiau.Models.Enums;
using System.Runtime.Serialization;
using System.Reflection;

namespace CaotinhoAuMiau.Utils
{
    public static class StatusAdocaoExtensions
    {
        public static string ObterTexto(this StatusAdocao status)
        {
            var field = typeof(StatusAdocao).GetField(status.ToString());
            var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();
            return attribute?.Value ?? status.ToString();
        }

        public static string ObterCssClass(this StatusAdocao status)
        {
            return status switch
            {
                StatusAdocao.Pendente => "status-pendente",
                StatusAdocao.Aprovado => "status-aprovado",
                StatusAdocao.ContratoAssinado => "status-contrato-assinado",
                StatusAdocao.AguardandoBuscar => "status-aguardando-buscar",
                StatusAdocao.Finalizado => "status-finalizado",
                StatusAdocao.Reprovado => "status-reprovado",
                StatusAdocao.Cancelado => "status-cancelado",
                StatusAdocao.CanceladoPeloCaotinho => "status-cancelado",
                StatusAdocao.CanceladoPorPrazoVencido => "status-cancelado",
                StatusAdocao.CanceladoPorNaoAssinarContrato => "status-cancelado",
                _ => "status-indefinido"
            };
        }

        public static string ObterIcone(this StatusAdocao status)
        {
            return status switch
            {
                StatusAdocao.Pendente => "fa-clock",
                StatusAdocao.Aprovado => "fa-check",
                StatusAdocao.ContratoAssinado => "fa-file-signature",
                StatusAdocao.AguardandoBuscar => "fa-calendar-check",
                StatusAdocao.Finalizado => "fa-flag-checkered",
                StatusAdocao.Reprovado => "fa-times",
                StatusAdocao.Cancelado => "fa-ban",
                StatusAdocao.CanceladoPeloCaotinho => "fa-ban",
                StatusAdocao.CanceladoPorPrazoVencido => "fa-ban",
                StatusAdocao.CanceladoPorNaoAssinarContrato => "fa-ban",
                _ => "fa-question"
            };
        }

        public static bool EstaCancelada(this StatusAdocao status)
        {
            return status == StatusAdocao.Cancelado ||
                   status == StatusAdocao.CanceladoPeloCaotinho ||
                   status == StatusAdocao.CanceladoPorPrazoVencido ||
                   status == StatusAdocao.CanceladoPorNaoAssinarContrato ||
                   status == StatusAdocao.Reprovado;
        }
    }
}