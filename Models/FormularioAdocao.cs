using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaotinhoAuMiau.Models
{
    public class FormularioAdocao
    {
        [Key]
        public int Id { get; set; }

        public int PetId { get; set; }
        public int UsuarioId { get; set; }

        [Required]
        public string EspacoAdequado { get; set; } = string.Empty;

        [Required]
        public string ExperienciaAnterior { get; set; } = string.Empty;

        [Required]
        public string MotivacaoAdocao { get; set; } = string.Empty;

        [Required]
        public string CondicoesFinanceiras { get; set; } = string.Empty;

        [Required]
        public string PlanejamentoViagens { get; set; } = string.Empty;

        [Required]
        [Range(1, double.MaxValue)]
        public decimal RendaMensal { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumeroMoradores { get; set; }

        [Required]
        public string DescricaoMoradia { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendente";
        public DateTime DataEnvio { get; set; } = DateTime.Now;
        public DateTime? DataResposta { get; set; }
        [MaxLength(500)]
        public string? ObservacaoAdminFormulario { get; set; }
        public string? ObservacoesCancelamento { get; set; }
        public virtual Pet? Pet { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
} 