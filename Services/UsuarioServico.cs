using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Data;
using System.Linq;
using CaotinhoAuMiau.Models.ViewModels.Usuario;

using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Services
{
    public class UsuarioServico
    {
        private readonly ApplicationDbContext _context;

        public UsuarioServico(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public UsuarioViewModel? ObterPorId(string idUsuario)
        {
            if (string.IsNullOrEmpty(idUsuario))
                throw new ArgumentException("ID do usuário não pode ser nulo ou vazio", nameof(idUsuario));

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Id.ToString() == idUsuario);

            return usuario != null ? UsuarioViewModel.CriarDeEntidade(usuario) : null;
        }

        public void Atualizar(UsuarioViewModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var usuario = _context.Usuarios.Find(model.Id);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado");

            usuario.Nome = model.Nome;
            usuario.Email = model.Email;
            usuario.Telefone = model.Telefone;
            usuario.CEP = model.CEP;
            usuario.Logradouro = model.Logradouro;
            usuario.Numero = model.Numero;
            usuario.Complemento = model.Complemento;
            usuario.Bairro = model.Bairro;
            usuario.Cidade = model.Cidade;
            usuario.Estado = model.Estado;
            usuario.FotoPerfil = model.FotoPerfil;
            usuario.DataAtualizacao = DateTime.Now;

            _context.SaveChanges();
        }

        public async Task AtualizarFotoPerfil(int idUsuario, string fotoUrl)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado");

            usuario.FotoPerfil = fotoUrl;
            usuario.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public bool ValidarSenha(string idUsuario, string senha)
        {
            var usuario = _context.Usuarios.Find(idUsuario);
            if (usuario == null)
            {
                return false;
            }

            return HashHelper.VerificarSenha(senha, usuario.Senha);
        }

        public void AlterarSenha(string idUsuario, string novaSenha)
        {
            var usuario = _context.Usuarios.Find(idUsuario);
            if (usuario == null)
            {
                throw new Exception("Usuário não encontrado");
            }

            usuario.Senha = HashHelper.GerarHashSenha(novaSenha);
            usuario.DataAtualizacao = DateTime.Now;

            _context.SaveChanges();
        }

    }
} 