using System.ComponentModel.DataAnnotations;

namespace CaotinhoAuMiau.Models.Enums
{
    public enum CargoColaboradorEnum
    {
        [Display(Name = "Administrador")]
        Administrador = 1,

        [Display(Name = "Colaborador")]
        Colaborador = 2,

        [Display(Name = "Voluntário")]
        Voluntario = 3
    }
}