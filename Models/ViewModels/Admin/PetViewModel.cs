using CaotinhoAuMiau.Models.ViewModels.Comuns;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Utils;
using System.ComponentModel.DataAnnotations;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class PetViewModel : PetViewModelBase
    {
        public Especie? FiltroEspecie { get; set; }
        public SexoPet? FiltroSexo { get; set; }
        public string FiltroPorte { get; set; } = string.Empty;
        public StatusPet? FiltroStatus { get; set; }

        public int? UsuarioId { get; set; } = 1;

        public int Idade { get => Anos; }

        [RequiredIfTrue(nameof(CadastroCompleto), ErrorMessage = "A espécie é obrigatória para cadastro completo.")]
        public new Especie? Especie { get; set; }

        [RequiredIfTrue(nameof(CadastroCompleto), ErrorMessage = "A raça é obrigatória para cadastro completo.")]
        [StringLength(50, ErrorMessage = "A raça não pode ter mais que 50 caracteres")]
        public new string? Raca { get; set; }

        [RequiredIfTrue(nameof(CadastroCompleto), ErrorMessage = "O sexo do pet é obrigatório para cadastro completo.")]
        public new SexoPet? Sexo { get; set; }

        [RequiredIfTrue(nameof(CadastroCompleto), ErrorMessage = "O porte do pet é obrigatório para cadastro completo.")]
        public new string? Porte { get; set; } = string.Empty;

        [RequiredIfTrue(nameof(CadastroCompleto), ErrorMessage = "A descrição é obrigatória para cadastro completo.")]
        [StringLength(500, ErrorMessage = "A descrição não pode ter mais que 500 caracteres")]
        public new string? Descricao { get; set; }

        [RequiredIfTrue(nameof(CadastroCompleto), ErrorMessage = "A imagem do pet é obrigatória para cadastro completo.")]
        public new string? NomeArquivoImagem { get; set; }

        public bool CadastroCompleto { get; set; }

    }
}
