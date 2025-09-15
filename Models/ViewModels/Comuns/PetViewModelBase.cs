using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.Enums;

namespace CaotinhoAuMiau.Models.ViewModels.Comuns
{
    public class PetViewModelBase
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do pet é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais que 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A espécie é obrigatória")]
        public Especie Especie { get; set; }

        [Required(ErrorMessage = "A raça é obrigatória")]
        [StringLength(50, ErrorMessage = "A raça não pode ter mais que 50 caracteres")]
        public string Raca { get; set; } = string.Empty;

        [Required(ErrorMessage = "A idade em anos é obrigatória")]
        [Range(0, 30, ErrorMessage = "A idade em anos deve estar entre 0 e 30")]
        public int Anos { get; set; }

        [Required(ErrorMessage = "Os meses são obrigatórios")]
        [Range(0, 11, ErrorMessage = "Os meses devem estar entre 0 e 11")]
        public int Meses { get; set; }

        public SexoPet Sexo { get; set; }

        public string Porte { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(500, ErrorMessage = "A descrição não pode ter mais que 500 caracteres")]
        public string Descricao { get; set; } = string.Empty;

        public StatusPet Status { get; set; } = StatusPet.Disponivel;

        public string? NomeArquivoImagem { get; set; }

        public List<Pet> Pets { get; set; } = new List<Pet>();
    }
}
