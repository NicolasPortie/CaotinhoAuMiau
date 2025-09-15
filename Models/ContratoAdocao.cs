using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaotinhoAuMiau.Models
{
    public class ContratoAdocao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdocaoId { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string ConteudoContrato { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? AssinaturaUsuario { get; set; }

        public DateTime? DataAssinatura { get; set; }

        [Required]
        [StringLength(20)]
        public string StatusContrato { get; set; } = "Pendente";

        [StringLength(500)]
        public string? CaminhoArquivoPdf { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public virtual Adocao? Adocao { get; set; }

        public bool EstaAssinado => StatusContrato == "Assinado" && DataAssinatura.HasValue;
        
        public bool EstaPendente => StatusContrato == "Pendente";
        
        public bool EstaExpirado => EstaPendente && DataCriacao.AddDays(7) < DateTime.Now;
    }
}