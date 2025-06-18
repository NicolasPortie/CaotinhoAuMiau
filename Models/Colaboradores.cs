using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaotinhoAuMiau.Models
{
    public class Colaborador
    {
        [Key]
        public int Id { get; set; }
        
        public int? UsuarioId { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O Nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Digite um endereço de email válido")]
        [StringLength(100, ErrorMessage = "O Email deve ter no máximo 100 caracteres")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A Senha deve ter no máximo 100 caracteres")]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter exatamente 11 dígitos")]
        [Display(Name = "CPF")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Telefone é obrigatório")]
        [StringLength(15, ErrorMessage = "O Telefone deve ter no máximo 15 caracteres")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Cargo é obrigatório")]
        [StringLength(50, ErrorMessage = "O Cargo deve ter no máximo 50 caracteres")]
        [Display(Name = "Cargo")]
        public string Cargo { get; set; } = string.Empty;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        
        [Display(Name = "Último Acesso")]
        public DateTime? UltimoAcesso { get; set; }
        
        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }
    }
} 