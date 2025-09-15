using System.Runtime.Serialization;

namespace CaotinhoAuMiau.Models.Enums
{
    public enum StatusPet
    {
        [EnumMember(Value = "Disponível")]
        Disponivel = 0,

        [EnumMember(Value = "Adotado")]
        Adotado = 1,

        [EnumMember(Value = "Em Processo")]
        EmProcesso = 2,

        [EnumMember(Value = "Rascunho")]
        Rascunho = 3
    }
}
