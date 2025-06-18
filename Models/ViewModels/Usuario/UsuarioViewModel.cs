using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O CPF é obrigatório")]
        [StringLength(11, ErrorMessage = "O CPF deve ter 11 dígitos")]
        [Display(Name = "CPF")]
        public string CPF { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome não pode ter mais que 100 caracteres")]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [StringLength(100, ErrorMessage = "O e-mail não pode ter mais que 100 caracteres")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [StringLength(15, ErrorMessage = "O telefone não pode ter mais que 15 caracteres")]
        [Display(Name = "Telefone")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de nascimento é obrigatória")]
        [DataType(DataType.Date)]
        [Display(Name = "Data de Nascimento")]
        public DateTime DataNascimento { get; set; } = DateTime.Now.AddYears(-18);

        [Required(ErrorMessage = "O CEP é obrigatório")]
        [StringLength(8)]
        [Display(Name = "CEP")]
        public string CEP { get; set; } = string.Empty;

        [Required(ErrorMessage = "O logradouro é obrigatório")]
        [StringLength(100, ErrorMessage = "O logradouro não pode ter mais que 100 caracteres")]
        [Display(Name = "Logradouro")]
        public string Logradouro { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número é obrigatório")]
        [StringLength(10, ErrorMessage = "O número não pode ter mais que 10 caracteres")]
        [Display(Name = "Número")]
        public string Numero { get; set; } = string.Empty;

        [Display(Name = "Complemento")]
        public string Complemento { get; set; } = string.Empty;

        [Required(ErrorMessage = "O bairro é obrigatório")]
        [StringLength(50, ErrorMessage = "O bairro não pode ter mais que 50 caracteres")]
        [Display(Name = "Bairro")]
        public string Bairro { get; set; } = string.Empty;

        [Required(ErrorMessage = "A cidade é obrigatória")]
        [StringLength(50, ErrorMessage = "A cidade não pode ter mais que 50 caracteres")]
        [Display(Name = "Cidade")]
        public string Cidade { get; set; } = string.Empty;

        [Required(ErrorMessage = "O estado é obrigatório")]
        [StringLength(2, ErrorMessage = "O estado deve ter 2 caracteres")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = string.Empty;

        [Display(Name = "Foto de Perfil")]
        public string? FotoPerfil { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }
        
        
        public bool Ativo { get; set; } = true;

        [NotMapped]
        [Compare("Senha", ErrorMessage = "As senhas não conferem")]
        public string ConfirmarSenha { get; set; } = string.Empty;

        public IFormFile? FotoPerfilFile { get; set; }

        public string ObterInicialNome()
        {
            if (string.IsNullOrEmpty(Nome))
                return "?";
                
            return Nome.Substring(0, 1).ToUpper();
        }

        public Models.Usuario ConverterParaEntidade()
        {
            return new Models.Usuario
            {
                Id = Id,
                CPF = CPF,
                Nome = Nome,
                Email = Email,
                Senha = Senha,
                Telefone = Telefone,
                DataNascimento = DataNascimento,
                CEP = CEP,
                Logradouro = Logradouro,
                Numero = Numero,
                Complemento = Complemento,
                Bairro = Bairro,
                Cidade = Cidade,
                Estado = Estado,
                FotoPerfil = FotoPerfil,
                DataCadastro = DataCadastro,
                DataAtualizacao = DataAtualizacao,
                Ativo = Ativo
            };
        }

        public static UsuarioViewModel CriarDeEntidade(Models.Usuario usuario)
        {
            return new UsuarioViewModel
            {
                Id = usuario.Id,
                CPF = usuario.CPF ?? string.Empty,
                Nome = usuario.Nome ?? string.Empty,
                Email = usuario.Email ?? string.Empty,
                Telefone = usuario.Telefone ?? string.Empty,
                DataNascimento = usuario.DataNascimento,
                CEP = usuario.CEP ?? string.Empty,
                Logradouro = usuario.Logradouro ?? string.Empty,
                Numero = usuario.Numero ?? string.Empty,
                Complemento = usuario.Complemento,
                Bairro = usuario.Bairro ?? string.Empty,
                Cidade = usuario.Cidade ?? string.Empty,
                Estado = usuario.Estado ?? string.Empty,
                FotoPerfil = usuario.FotoPerfil,
                DataCadastro = usuario.DataCadastro,
                DataAtualizacao = usuario.DataAtualizacao,
                Ativo = usuario.Ativo
            };
        }
    }
} 