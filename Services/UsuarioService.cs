using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(ApplicationDbContext contexto, ILogger<UsuarioService> logger)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
            _logger = logger;
        }


        public async Task<Usuario?> AutenticarAsync(string email, string senha)
        {
            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

            if (usuario != null && HashHelper.VerificarSenha(senha, usuario.Senha))
                return usuario;

            return null;
        }

        public async Task<Usuario> RegistrarUsuarioAsync(Usuario usuario)
        {
            usuario.Senha = HashHelper.GerarHashSenha(usuario.Senha);
            usuario.DataCadastro = DateTime.Now;
            usuario.Ativo = true;
            _contexto.Usuarios.Add(usuario);
            await _contexto.SaveChangesAsync();
            return usuario;
        }

        public async Task<bool> CPFExisteAsync(string cpf)
        {
            return await _contexto.Usuarios.AnyAsync(u => u.CPF == cpf);
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            return await _contexto.Usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task<Usuario?> ObterUsuarioPorIdAsync(int id)
        {
            return await _contexto.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> ObterUsuarioPorEmailAsync(string email)
        {
            return await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailJaExisteAsync(string email)
        {
            return await _contexto.Usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> AlterarSenhaUsuarioAsync(string email, string novaSenha)
        {
            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario != null)
            {
                usuario.Senha = HashHelper.GerarHashSenha(novaSenha);
                await _contexto.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task AtualizarUltimoAcessoUsuarioAsync(Usuario usuario)
        {
            usuario.UltimoAcesso = DateTime.Now;
            await _contexto.SaveChangesAsync();
        }
    }
}