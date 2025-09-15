using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CaotinhoAuMiau.Models.Enums;

namespace CaotinhoAuMiau.Models
{
    public class Adocao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PetId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;
        public DateTime? DataResposta { get; set; }
        public DateTime? DataFinalizacao { get; set; }
        public StatusAdocao Status { get; set; } = StatusAdocao.AguardandoAssinarContrato;
        [MaxLength(300)]
        public string? ObservacoesCancelamento { get; set; }
        
        [MaxLength(300)]
        public string? ObservacoesReativacao { get; set; }


        public int? ContratoId { get; set; }
        public bool ContratoAssinado { get; set; } = false;

        public virtual Pet? Pet { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public virtual ContratoAdocao? Contrato { get; set; }
    }
} 