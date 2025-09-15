using System.ComponentModel;
using System.Runtime.Serialization;

namespace CaotinhoAuMiau.Models.Enums
{
    public enum Especie
    {
        [EnumMember(Value = "Cao")]
        [Description("Cachorro")]
        Cao = 0,
        
        [EnumMember(Value = "Felino")]
        [Description("Gato")]
        Felino = 1
    }
}
