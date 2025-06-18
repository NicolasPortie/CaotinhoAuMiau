using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CaotinhoAuMiau.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }

        public int? UsuarioId { get; set; }

        [Required(ErrorMessage = "O nome do pet é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais que 100 caracteres")]
        public string Nome { get; set; } = string.Empty;
        public string? Especie { get; set; } = "";

        [StringLength(50, ErrorMessage = "A raça não pode ter mais que 50 caracteres")]
        public string? Raca { get; set; } = "";

        [Range(0, 30, ErrorMessage = "A idade em anos deve estar entre 0 e 30")]
        public int Anos { get; set; }

        [Range(0, 11, ErrorMessage = "Os meses devem estar entre 0 e 11")]
        public int Meses { get; set; }
        
        public int Idade { get => Anos; }
        public string? Sexo { get; set; } = "";
        public string? Porte { get; set; } = "";

        [StringLength(500, ErrorMessage = "A descrição não pode ter mais que 500 caracteres")]
        public string? Descricao { get; set; } = "";

        public string? Status { get; set; } = "";

        public string? NomeArquivoImagem { get; set; }
        public bool CadastroCompleto { get; set; } = false;
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? DataAtualizacao { get; set; }

        public virtual ICollection<FormularioAdocao> FormulariosAdocao { get; set; } = new List<FormularioAdocao>();
        public virtual ICollection<Adocao> Adocoes { get; set; } = new List<Adocao>();
    }
} 