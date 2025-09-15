using System.Threading.Tasks;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> AutenticarAsync(string email, string senha);
        Task<Usuario> RegistrarUsuarioAsync(Usuario usuario);
        Task<bool> CPFExisteAsync(string cpf);
        Task<bool> EmailExisteAsync(string email);
        Task<Usuario?> ObterUsuarioPorIdAsync(int id);
        Task<Usuario?> ObterUsuarioPorEmailAsync(string email);
        Task<bool> EmailJaExisteAsync(string email);
        Task<bool> AlterarSenhaUsuarioAsync(string email, string novaSenha);
        Task AtualizarUltimoAcessoUsuarioAsync(Usuario usuario);
    }
}
