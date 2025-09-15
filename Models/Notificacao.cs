using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaotinhoAuMiau.Models
{
    public class Notificacao
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Mensagem { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Tipo { get; set; }

        [StringLength(50)]
        public string? Referencia { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public bool Lida { get; set; } = false;
        public DateTime? DataLeitura { get; set; }

        public virtual Usuario? Usuario { get; set; }
    }
} 