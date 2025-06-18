using System;
using System.ComponentModel.DataAnnotations;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class ColaboradorViewModel
    {
        public int Id { get; set; }
        public int? UsuarioId { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais que 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [StringLength(100, ErrorMessage = "O e-mail não pode ter mais que 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha não pode ter mais que 100 caracteres")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter 11 dígitos")]
        public string CPF { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string SenhaAtual { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string ConfirmarSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [StringLength(15, ErrorMessage = "O telefone não pode ter mais que 15 caracteres")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cargo é obrigatório")]
        [StringLength(50, ErrorMessage = "O cargo não pode ter mais que 50 caracteres")]
        public string Cargo { get; set; } = "Colaborador";

        public bool Ativo { get; set; } = true;
        
        [Display(Name = "Data de Cadastro")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Display(Name = "Último Acesso")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? UltimoAcesso { get; set; }
        
        public Colaborador ConverterParaEntidade()
        {
            return new Colaborador
            {
                Id = Id,
                UsuarioId = UsuarioId,
                Nome = Nome,
                CPF = CPF,
                Email = Email,
                Telefone = Telefone,
                Cargo = Cargo,
                Senha = Senha,
                Ativo = Ativo,
                DataCadastro = DataCadastro,
                UltimoAcesso = UltimoAcesso
            };
        }

        public static ColaboradorViewModel CriarDeEntidade(Colaborador entidade)
        {
            return new ColaboradorViewModel
            {
                Id = entidade.Id,
                UsuarioId = entidade.UsuarioId,
                Nome = entidade.Nome,
                CPF = entidade.CPF,
                Email = entidade.Email,
                Telefone = entidade.Telefone,
                Cargo = entidade.Cargo,
                Senha = string.Empty,
                Ativo = entidade.Ativo,
                DataCadastro = entidade.DataCadastro,
                UltimoAcesso = entidade.UltimoAcesso
            };
        }
    }
} 