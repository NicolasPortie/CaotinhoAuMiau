using System.ComponentModel.DataAnnotations;

namespace CaotinhoAuMiau.Models
{
    public class Log
    {
        public int Id { get; set; }

        [Required]
        public DateTime DataHora { get; set; }

        [Required]
        [StringLength(100)]
        public string UsuarioEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string UsuarioNome { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PerfilUsuario { get; set; } = string.Empty; // Admin, Colaborador, Voluntário, Usuário

        [Required]
        [StringLength(50)]
        public string TipoAcao { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty; // Pet, Adoção, Usuário, Sistema, Email, etc.

        [Required]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(50)]
        public string? EntidadeAfetada { get; set; }

        public int? EntidadeId { get; set; }

        [StringLength(50)]
        public string NivelSeveridade { get; set; } = "Info"; // Info, Warning, Error, Critical

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(2000)]
        public string? DetalhesAdicionais { get; set; }

        public DateTime? DataExclusao { get; set; }

        public bool Ativo { get; set; } = true;
    }
}