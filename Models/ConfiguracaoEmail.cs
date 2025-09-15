using System.ComponentModel.DataAnnotations;

namespace CaotinhoAuMiau.Models
{
    public class ConfiguracaoEmail
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O servidor SMTP é obrigatório")]
        [StringLength(100, ErrorMessage = "O servidor SMTP deve ter no máximo 100 caracteres")]
        [Display(Name = "Servidor SMTP")]
        public string ServidorSmtp { get; set; } = string.Empty;

        [Required(ErrorMessage = "A porta é obrigatória")]
        [Range(1, 65535, ErrorMessage = "A porta deve estar entre 1 e 65535")]
        [Display(Name = "Porta")]
        public int Porta { get; set; } = 587;

        [Required(ErrorMessage = "O email remetente é obrigatório")]
        [EmailAddress(ErrorMessage = "Digite um endereço de email válido")]
        [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres")]
        [Display(Name = "Email Remetente")]
        public string EmailRemetente { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome remetente é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome Remetente")]
        public string NomeRemetente { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(200, ErrorMessage = "A senha deve ter no máximo 200 caracteres")]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Display(Name = "Usar SSL")]
        public bool UsarSsl { get; set; } = true;

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Display(Name = "Data de Atualização")]
        public DateTime? DataAtualizacao { get; set; }

        [EmailAddress(ErrorMessage = "Digite um endereço de email válido")]
        [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres")]
        [Display(Name = "Email de Teste")]
        public string? EmailTeste { get; set; }
    }
}