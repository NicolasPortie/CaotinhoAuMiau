using System;
using System.ComponentModel.DataAnnotations;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class AvaliacaoAdocaoViewModel
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "A resposta sobre espaço adequado é obrigatória")]
        [StringLength(500, ErrorMessage = "A resposta deve ter no máximo 500 caracteres")]
        public string EspacoAdequado { get; set; } = string.Empty;

        [Required(ErrorMessage = "A resposta sobre experiência anterior é obrigatória")]
        [StringLength(500, ErrorMessage = "A resposta deve ter no máximo 500 caracteres")]
        public string ExperienciaAnterior { get; set; } = string.Empty;

        [Required(ErrorMessage = "A motivação para adoção é obrigatória")]
        [StringLength(500, ErrorMessage = "A motivação deve ter no máximo 500 caracteres")]
        public string MotivacaoAdocao { get; set; } = string.Empty;

        [Required(ErrorMessage = "As condições financeiras são obrigatórias")]
        [StringLength(500, ErrorMessage = "A resposta deve ter no máximo 500 caracteres")]
        public string CondicoesFinanceiras { get; set; } = string.Empty;

        [Required(ErrorMessage = "O planejamento de viagens é obrigatório")]
        [StringLength(500, ErrorMessage = "A resposta deve ter no máximo 500 caracteres")]
        public string PlanejamentoViagens { get; set; } = string.Empty;

        [Required(ErrorMessage = "A renda mensal é obrigatória")]
        [Range(1, double.MaxValue, ErrorMessage = "A renda mensal deve ser maior que zero")]
        public decimal RendaMensal { get; set; }

        [Required(ErrorMessage = "O número de moradores é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "O número de moradores deve ser maior que zero")]
        public int NumeroMoradores { get; set; }

        [Required(ErrorMessage = "A descrição da moradia é obrigatória")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string DescricaoMoradia { get; set; } = string.Empty;

        public string Status { get; set; } = "Pendente";
        public DateTime DataEnvio { get; set; } = DateTime.Now;
        public DateTime? DataResposta { get; set; }
        public string? ObservacaoAdminFormulario { get; set; }

    }
} 