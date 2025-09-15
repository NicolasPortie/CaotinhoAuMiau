using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Utils;

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

        public int? TempoDisponivel { get; set; }
        
        [StringLength(10)]
        public string? TevePetAnterior { get; set; }

        public StatusFormulario StatusEnum { get; set; } = StatusFormulario.Pendente;
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Status => StatusEnum.ObterValorMembroEnum();
        public DateTime DataEnvio { get; set; } = DateTime.Now;
        public DateTime? DataResposta { get; set; }
        [MaxLength(500)]
        public string? ObservacaoAdminFormulario { get; set; }
        [MaxLength(300)]
        public string? ObservacoesCancelamento { get; set; }
        
        [NotMapped]
        public int? AdocaoId { get; set; }
        
        public virtual Pet? Pet { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
} 