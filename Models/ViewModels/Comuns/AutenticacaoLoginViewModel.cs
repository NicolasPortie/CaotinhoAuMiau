using System.ComponentModel.DataAnnotations;

namespace CaotinhoAuMiau.Models.ViewModels.Comuns
{
    public class AutenticacaoLoginViewModel
    {
        [Required(ErrorMessage = "O Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Digite um endereço de email válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Senha é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;
        
        [Display(Name = "Continuar conectado")]
        public bool ContinuarConectado { get; set; } = false;
    }
} 