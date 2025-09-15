using System.Runtime.Serialization;

namespace CaotinhoAuMiau.Models.Enums
{
    public enum StatusAdocao
    {
        [EnumMember(Value = "Pendente")]
        Pendente,

        [EnumMember(Value = "Aprovado")]
        Aprovado,

        [EnumMember(Value = "Reprovado")]
        Reprovado,

        [EnumMember(Value = "Cancelado")]
        Cancelado,

        [EnumMember(Value = "Aguardando Assinar Contrato")]
        AguardandoAssinarContrato,

        [EnumMember(Value = "Contrato Assinado")]
        ContratoAssinado,

        [EnumMember(Value = "Aguardando Buscar")]
        AguardandoBuscar,

        [EnumMember(Value = "Finalizado")]
        Finalizado,

        [EnumMember(Value = "Cancelado pelo Caotinho")]
        CanceladoPeloCaotinho,

        [EnumMember(Value = "Cancelado por Prazo Vencido")]
        CanceladoPorPrazoVencido,

        [EnumMember(Value = "Cancelado por Não Assinar Contrato")]
        CanceladoPorNaoAssinarContrato
    }
}