using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Controllers.Usuario
{
    [Route("usuario/perfil")]
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly IWebHostEnvironment _ambienteHost;
        private readonly UsuarioServico _servicoUsuario;
        private readonly ILogger<PerfilController> _logger;

        public PerfilController(ApplicationDbContext contexto, IWebHostEnvironment ambienteHost, UsuarioServico servicoUsuario, ILogger<PerfilController> logger)
        {
            _contexto = contexto;
            _ambienteHost = ambienteHost;
            _servicoUsuario = servicoUsuario;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> ExibirPerfil()
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToAction("ExibirTelaLogin", "Autenticacao");
            }

            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id.ToString() == idUsuario);
            if (usuario == null)
            {
                return NotFound();
            }

            var viewModel = new UsuarioViewModel
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                CPF = usuario.CPF,
                Telefone = usuario.Telefone,
                DataNascimento = usuario.DataNascimento,
                CEP = usuario.CEP,
                Logradouro = usuario.Logradouro,
                Numero = usuario.Numero,
                Complemento = usuario.Complemento,
                Bairro = usuario.Bairro,
                Cidade = usuario.Cidade,
                Estado = usuario.Estado,
                FotoPerfil = usuario.FotoPerfil
            };

            return View("~/Views/Usuario/Perfil.cshtml", viewModel);
        }

        [HttpPost]
        [Route("atualizar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarAtualizacaoPerfil(UsuarioViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Usuario/Perfil.cshtml", modelo);
            }

            var usuario = await _contexto.Usuarios.FindAsync(modelo.Id);
            if (usuario == null)
            {
                return NotFound();
            }

            var idUsuario = User.ObterIdUsuario();
            if (usuario.Id.ToString() != idUsuario)
            {
                return Forbid();
            }

            if (modelo.FotoPerfilFile != null && modelo.FotoPerfilFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    var caminhoAntigo = Path.Combine(_ambienteHost.WebRootPath, "imagens", "perfil", usuario.FotoPerfil);
                    if (System.IO.File.Exists(caminhoAntigo))
                    {
                        try
                        {
                            System.IO.File.Delete(caminhoAntigo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao remover imagem antiga de perfil");
                        }
                    }
                }

                string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(modelo.FotoPerfilFile.FileName);
                string caminho = Path.Combine(_ambienteHost.WebRootPath, "imagens", "perfil", nomeArquivo);
                
                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    await modelo.FotoPerfilFile.CopyToAsync(stream);
                }

                usuario.FotoPerfil = nomeArquivo;
            }

            usuario.Nome = modelo.Nome;
            usuario.Email = modelo.Email;
            usuario.Telefone = modelo.Telefone;
            usuario.CEP = modelo.CEP;
            usuario.Logradouro = modelo.Logradouro;
            usuario.Numero = modelo.Numero;
            usuario.Complemento = modelo.Complemento;
            usuario.Bairro = modelo.Bairro;
            usuario.Cidade = modelo.Cidade;
            usuario.Estado = modelo.Estado;
            usuario.DataAtualizacao = DateTime.Now;

            try
            {
                _contexto.Usuarios.Update(usuario);
                await _contexto.SaveChangesAsync();
                TempData["Sucesso"] = "Perfil atualizado com sucesso!";
                return RedirectToAction(nameof(ExibirPerfil));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ocorreu um erro ao atualizar o perfil: {ex.Message}");
                return View("~/Views/Usuario/Perfil.cshtml", modelo);
            }
        }

        [HttpPost]
        [Route("remover-foto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverFotoPerfil()
        {
            try
            {
                var idUsuario = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(idUsuario))
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Usuário não autenticado" });
                    }
                    return RedirectToAction("ExibirTelaLogin", "Autenticacao");
                }

                var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id.ToString() == idUsuario);
                if (usuario == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Usuário não encontrado" });
                    }
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    var caminhoAntigo = Path.Combine(_ambienteHost.WebRootPath, "imagens", "perfil", usuario.FotoPerfil);
                    if (System.IO.File.Exists(caminhoAntigo))
                    {
                        try
                        {
                            System.IO.File.Delete(caminhoAntigo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao excluir foto de perfil");
                        }
                    }

                    usuario.FotoPerfil = null;
                    _contexto.Usuarios.Update(usuario);
                    await _contexto.SaveChangesAsync();
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    
                    TempData["Sucesso"] = "Foto de perfil removida com sucesso!";
                }
                else if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Usuário não possui foto de perfil" });
                }

                return RedirectToAction(nameof(ExibirPerfil));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Erro ao remover foto: {ex.Message}" });
                }
                
                TempData["Erro"] = $"Erro ao remover foto: {ex.Message}";
                return RedirectToAction(nameof(ExibirPerfil));
            }
        }

        [HttpPost]
        [Route("alterar-senha")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarAlteracaoSenha(string SenhaAtual, string NovaSenha, string ConfirmarSenha)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToAction("ExibirTelaLogin", "Autenticacao");
            }

            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id.ToString() == idUsuario);
            if (usuario == null)
            {
                return NotFound();
            }

            if (!HashHelper.VerificarSenha(SenhaAtual, usuario.Senha))
            {
                ModelState.AddModelError("", "A senha atual está incorreta.");
                return View("~/Views/Usuario/Perfil.cshtml");
            }

            if (NovaSenha != ConfirmarSenha)
            {
                ModelState.AddModelError("", "A nova senha e a confirmação não coincidem.");
                return View("~/Views/Usuario/Perfil.cshtml");
            }

            usuario.Senha = HashHelper.GerarHashSenha(NovaSenha);
            usuario.DataAtualizacao = DateTime.Now;

            try
            {
                _contexto.Usuarios.Update(usuario);
                await _contexto.SaveChangesAsync();
                TempData["Sucesso"] = "Senha alterada com sucesso!";
                return RedirectToAction(nameof(ExibirPerfil));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ocorreu um erro ao atualizar a senha: {ex.Message}");
                return View("~/Views/Usuario/Perfil.cshtml");
            }
        }

        [HttpPost]
        [Route("atualizarFoto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarFotoPerfil()
        {
            try
            {
                var idUsuario = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(idUsuario))
                {
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id.ToString() == idUsuario);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuário não encontrado" });
                }

                var arquivo = Request.Form.Files["FotoPerfil"];
                if (arquivo == null || arquivo.Length == 0)
                {
                    return Json(new { success = false, message = "Nenhum arquivo selecionado" });
                }

                if (arquivo.Length > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "A imagem deve ter até 5MB" });
                }

                if (!arquivo.ContentType.StartsWith("image/"))
                {
                    return Json(new { success = false, message = "Por favor, selecione um arquivo de imagem válido" });
                }

                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    var caminhoAntigo = Path.Combine(_ambienteHost.WebRootPath, "imagens", "perfil", usuario.FotoPerfil);
                    if (System.IO.File.Exists(caminhoAntigo))
                    {
                        try
                        {
                            System.IO.File.Delete(caminhoAntigo);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao remover imagem de perfil");
                        }
                    }
                }

                string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(arquivo.FileName);
                string caminho = Path.Combine(_ambienteHost.WebRootPath, "imagens", "perfil", nomeArquivo);
                
                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    await arquivo.CopyToAsync(stream);
                }

                usuario.FotoPerfil = nomeArquivo;
                usuario.DataAtualizacao = DateTime.Now;

                _contexto.Usuarios.Update(usuario);
                await _contexto.SaveChangesAsync();

                return Json(new { success = true, fileName = nomeArquivo });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao atualizar foto: {ex.Message}" });
            }
        }
    }
} 