using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class PetViewModel
    {
        public List<Pet> Pets { get; set; } = new List<Pet>();

        public string FiltroEspecie { get; set; } = string.Empty;
        public string FiltroSexo { get; set; } = string.Empty;
        public string FiltroPorte { get; set; } = string.Empty;
        public string FiltroStatus { get; set; } = string.Empty;

        public int Id { get; set; }
        public int? UsuarioId { get; set; } = 1;

        [Required(ErrorMessage = "O nome do pet é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais que 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A espécie é obrigatória")]
        public string Especie { get; set; } = string.Empty;

        [Required(ErrorMessage = "A raça é obrigatória")]
        [StringLength(50, ErrorMessage = "A raça não pode ter mais que 50 caracteres")]
        public string Raca { get; set; } = string.Empty;

        [Required(ErrorMessage = "A idade em anos é obrigatória")]
        [Range(0, 30, ErrorMessage = "A idade em anos deve estar entre 0 e 30")]
        public int Anos { get; set; }

        [Required(ErrorMessage = "Os meses são obrigatórios")]
        [Range(0, 11, ErrorMessage = "Os meses devem estar entre 0 e 11")]
        public int Meses { get; set; }

        public int Idade { get => Anos; }

        [Required(ErrorMessage = "O sexo é obrigatório")]
        public string Sexo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O porte é obrigatório")]
        public string Porte { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(500, ErrorMessage = "A descrição não pode ter mais que 500 caracteres")]
        public string Descricao { get; set; } = string.Empty;

        public string NomeArquivoImagem { get; set; } = string.Empty;
        public bool CadastroCompleto { get; set; }


    }
} 