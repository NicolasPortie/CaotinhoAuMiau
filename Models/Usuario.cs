using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaotinhoAuMiau.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório")]
        [StringLength(11, ErrorMessage = "O CPF deve ter 11 dígitos")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais que 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [StringLength(100, ErrorMessage = "O e-mail não pode ter mais que 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [StringLength(15, ErrorMessage = "O telefone não pode ter mais que 15 caracteres")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        [Required(ErrorMessage = "O CEP é obrigatório")]
        [StringLength(8, ErrorMessage = "O CEP deve ter 8 dígitos")]
        public string CEP { get; set; } = string.Empty;

        [Required(ErrorMessage = "O logradouro é obrigatório")]
        [StringLength(100, ErrorMessage = "O logradouro não pode ter mais que 100 caracteres")]
        public string Logradouro { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número é obrigatório")]
        [StringLength(10, ErrorMessage = "O número não pode ter mais que 10 caracteres")]
        public string Numero { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "O complemento não pode ter mais que 50 caracteres")]
        public string? Complemento { get; set; }

        [Required(ErrorMessage = "O bairro é obrigatório")]
        [StringLength(50, ErrorMessage = "O bairro não pode ter mais que 50 caracteres")]
        public string Bairro { get; set; } = string.Empty;

        [Required(ErrorMessage = "A cidade é obrigatória")]
        [StringLength(50, ErrorMessage = "A cidade não pode ter mais que 50 caracteres")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "O estado é obrigatório")]
        [StringLength(2, ErrorMessage = "O estado deve ter 2 caracteres")]
        public string Estado { get; set; } = string.Empty;
        
        public DateTime DataCadastro { get; set; } = DateTime.Now;  
        public DateTime? DataAtualizacao { get; set; }  
        

        [NotMapped]
        [Compare("Senha", ErrorMessage = "As senhas não conferem")]
        public string ConfirmarSenha { get; set; } = string.Empty;
        
        public bool Ativo { get; set; } = true;

        public DateTime? UltimoAcesso { get; set; }

        public string? FotoPerfil { get; set; }
        public virtual ICollection<FormularioAdocao> FormulariosAdocao { get; set; } = new List<FormularioAdocao>();
        public virtual ICollection<Adocao> Adocoes { get; set; } = new List<Adocao>();
        public virtual ICollection<Notificacao> Notificacoes { get; set; } = new List<Notificacao>();
    }
} 