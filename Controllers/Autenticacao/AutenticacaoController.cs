using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.ViewModels;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Controllers.Autenticacao
{
    [Route("usuario")]
    public class AutenticacaoController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<AutenticacaoController> _logger;

        public AutenticacaoController(ApplicationDbContext contexto, ILogger<AutenticacaoController> logger)
        {
            _contexto = contexto;
            _logger = logger;
            _ = Task.Run(CriarAdminPadrao);
        }

        private async Task CriarAdminPadrao()
        {
            try
            {
                var colaboradorExistente = await _contexto.Colaboradores
                    .FirstOrDefaultAsync(a => a.Email == "admin@admin.com");

                if (colaboradorExistente == null)
                {
                    var colaborador = new Colaborador
                    {
                        Nome = "Administrador",
                        Email = "admin@admin.com",
                        Senha = HashHelper.GerarHashSenha("admin"),
                        CPF = "00000000000",
                        Telefone = "0000000000",
                        Cargo = "Administrador",
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };

                    _contexto.Colaboradores.Add(colaborador);
                    await _contexto.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao criar admin padrão: {ex.Message}");
            }
        }

        [HttpGet("login")]
        public IActionResult ExibirTelaLogin(string returnUrl = "", bool cadastroSucesso = false)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador"))
                {
                    return RedirectToAction("Inicio", "GerenciamentoDashboard");
                }
                else
                {
                    return Redirect("/usuario/pets/explorar");
                }
            }
            
            if (cadastroSucesso && !TempData.ContainsKey("Sucesso"))
            {
                TempData["Sucesso"] = "Cadastro realizado com sucesso! Você já pode fazer login para acessar sua conta.";
            }
            
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Autenticacao/Login.cshtml");
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarLogin(CaotinhoAuMiau.Models.ViewModels.Comuns.AutenticacaoLoginViewModel modelo, string returnUrl = "")
        {
            System.Diagnostics.Debug.WriteLine($"==== PROCESSANDO LOGIN ====");
            System.Diagnostics.Debug.WriteLine($"ReturnUrl recebida: {returnUrl}");
            
            if (User?.Identity?.IsAuthenticated == true)
            {
                System.Diagnostics.Debug.WriteLine($"Usuário já está autenticado");
                
                if (User.IsInRole("Administrador"))
                {
                    System.Diagnostics.Debug.WriteLine($"Usuário é administrador, redirecionando para /admin/index");
                    return RedirectToAction("Index", "Painel", new { area = "Admin" });
                }
                
                System.Diagnostics.Debug.WriteLine($"Usuário não é administrador, redirecionando para página inicial");
                return RedirectToAction("Index", "Home");
            }
            
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"Modelo de dados inválido, retornando para a tela de login");
                return View("~/Views/Autenticacao/Login.cshtml", modelo);
            }
            
            try
            {
                _logger.LogInformation("Tentativa de login para email: {Email}", modelo.Email);
                
                var colaborador = await _contexto.Colaboradores
                    .FirstOrDefaultAsync(c => c.Email == modelo.Email);

                if (colaborador != null)
                {
                    _logger.LogInformation("Encontrou colaborador com email {Email}", modelo.Email);
                    
                    if (HashHelper.VerificarSenha(modelo.Senha, colaborador.Senha))
                    {
                        _logger.LogInformation("Senha correta para colaborador {Email}", modelo.Email);
                        
                        if (colaborador.Ativo)
                        {
                            _logger.LogInformation("Colaborador está ativo, criando claims para {Email}", modelo.Email);
                            
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, colaborador.Id.ToString()),
                                new Claim(ClaimTypes.Name, colaborador.Nome),
                                new Claim(ClaimTypes.Email, colaborador.Email),
                                new Claim(ClaimTypes.Role, "Administrador"),
                                new Claim("TipoPerfil", "Administrador"),
                                new Claim("Cargo", colaborador.Cargo)
                            };
                            
                            var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            
                            var propriedadesAutenticacao = new AuthenticationProperties
                            {
                                IsPersistent = modelo.ContinuarConectado,
                                ExpiresUtc = modelo.ContinuarConectado ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(2)
                            };
                            
                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(identidade),
                                propriedadesAutenticacao);
                            
                            colaborador.UltimoAcesso = DateTime.Now;
                            await _contexto.SaveChangesAsync();
                            
                            _logger.LogInformation("Login de administrador bem-sucedido, redirecionando para dashboard");
                            
                            return RedirectToAction("Inicio", "GerenciamentoDashboard", new { area = "" });
                        }
                        else
                        {
                            _logger.LogWarning("Tentativa de login com conta de administrador inativa: {Email}", modelo.Email);
                            TempData["Erro"] = "Esta conta de administrador está inativa. Entre em contato com o suporte.";
                            
                            return View("~/Views/Autenticacao/Login.cshtml", modelo);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Tentativa de login com senha incorreta para administrador: {Email}", modelo.Email);
                        TempData["Erro"] = "Credenciais inválidas. Por favor, verifique seu email e senha.";
                        
                        return View("~/Views/Autenticacao/Login.cshtml", modelo);
                    }
                }
                
                var usuario = await _contexto.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == modelo.Email);
                
                if (usuario != null)
                {
                    _logger.LogInformation("Encontrou usuário com email {Email}", modelo.Email);
                    
                    if (HashHelper.VerificarSenha(modelo.Senha, usuario.Senha))
                    {
                        _logger.LogInformation("Senha correta para usuário {Email}", modelo.Email);
                        
                        if (usuario.Ativo)
                        {
                            _logger.LogInformation("Usuário está ativo, criando claims para {Email}", modelo.Email);
                            
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                                new Claim(ClaimTypes.Name, usuario.Nome),
                                new Claim(ClaimTypes.Email, usuario.Email),
                                new Claim(ClaimTypes.Role, "Usuario"),
                                new Claim("TipoPerfil", "Usuario")
                            };
                            
                            var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            
                            var propriedadesAutenticacao = new AuthenticationProperties
                            {
                                IsPersistent = modelo.ContinuarConectado,
                                ExpiresUtc = modelo.ContinuarConectado ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(2)
                            };
                            
                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(identidade),
                                propriedadesAutenticacao);
                            
                            usuario.UltimoAcesso = DateTime.Now;
                            await _contexto.SaveChangesAsync();
                            
                            _logger.LogInformation("Login de usuário bem-sucedido para {Email}", modelo.Email);
                            
                            string? urlRedirecionamento = null;
                            
                            if (TempData["RedirectToPetId"] != null)
                            {
                                var petIdTemp = TempData["RedirectToPetId"];
                                if (petIdTemp != null)
                                {
                                    var petIdStr = petIdTemp.ToString();
                                    System.Diagnostics.Debug.WriteLine($"PetId encontrado no TempData: {petIdStr}");
                                    if (!string.IsNullOrEmpty(petIdStr))
                                    {
                                        urlRedirecionamento = $"/usuario/adocao/formulario/{petIdStr}";
                                    }
                                }
                            }
                            
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            {
                                System.Diagnostics.Debug.WriteLine($"Usando returnUrl para redirecionamento: {returnUrl}");
                                urlRedirecionamento = returnUrl;
                            }
                            
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.Contains("petId="))
                            {
                                System.Diagnostics.Debug.WriteLine($"ReturnUrl contém petId como parâmetro");
                                var match = System.Text.RegularExpressions.Regex.Match(returnUrl, @"petId=(\d+)");
                                if (match.Success)
                                {
                                    var petIdFromUrl = match.Groups[1].Value;
                                    System.Diagnostics.Debug.WriteLine($"PetId extraído da URL: {petIdFromUrl}");
                                    urlRedirecionamento = $"/usuario/adocao/formulario/{petIdFromUrl}";
                                }
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"URL final de redirecionamento: {urlRedirecionamento}");
                            
                            if (!string.IsNullOrEmpty(urlRedirecionamento))
                            {
                                return Redirect(urlRedirecionamento);
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Tentativa de login com conta de usuário inativa: {Email}", modelo.Email);
                            TempData["Erro"] = "Esta conta está inativa. Entre em contato com o suporte.";
                            
                            return View("~/Views/Autenticacao/Login.cshtml", modelo);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Tentativa de login com senha incorreta para usuário: {Email}", modelo.Email);
                        TempData["Erro"] = "Credenciais inválidas. Por favor, verifique seu email e senha.";
                        
                        return View("~/Views/Autenticacao/Login.cshtml", modelo);
                    }
                }
                
                _logger.LogWarning("Tentativa de login com email não cadastrado: {Email}", modelo.Email);
                TempData["Erro"] = "Credenciais inválidas. Por favor, verifique seu email e senha.";
                
                return View("~/Views/Autenticacao/Login.cshtml", modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar login para {Email}", modelo.Email);
                TempData["Erro"] = "Ocorreu um erro ao processar o login. Por favor, tente novamente.";
                
                return View("~/Views/Autenticacao/Login.cshtml", modelo);
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> EncerrarSessao()
        {
            try
            {
                _logger.LogInformation("Iniciando processo de logout para usuário: {NomeUsuario}", User?.Identity?.Name ?? "Desconhecido");
                
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                HttpContext.Session.Clear();
                
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }
                
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1),
                    SameSite = SameSiteMode.Lax,
                    Secure = Request.IsHttps,
                    HttpOnly = true
                };
                
                Response.Cookies.Append(CookieAuthenticationDefaults.AuthenticationScheme, "", cookieOptions);
                Response.Cookies.Append("CaotinhoAuMiau.Auth", "", cookieOptions);
                Response.Cookies.Append(".AspNetCore.Cookies", "", cookieOptions);
                
                _logger.LogInformation("Logout concluído com sucesso");
                
                TempData["Sucesso"] = "Você foi desconectado com sucesso!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar logout");
                return RedirectToAction("Index", "Home");
            }
        }



        #region API para validação de formulários

        [HttpGet("verificar-cpf")]
        public async Task<IActionResult> VerificarCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return Json(new { valido = false, mensagem = "CPF não informado." });
            }

            cpf = Regex.Replace(cpf, @"[^\d]", "");

            var cpfExiste = await _contexto.Usuarios.AnyAsync(u => u.CPF == cpf);
            
            return Json(new { valido = !cpfExiste, mensagem = cpfExiste ? "Este CPF já está cadastrado." : "" });
        }

        [HttpGet("verificar-email")]
        public async Task<IActionResult> VerificarEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return Json(new { emailExiste = false, mensagem = "Email não informado" });
                }

                var usuarioExistente = await _contexto.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email);

                var colaboradorExistente = await _contexto.Colaboradores
                    .FirstOrDefaultAsync(a => a.Email == email);

                return Json(new { 
                    emailExiste = (usuarioExistente != null || colaboradorExistente != null),
                    mensagem = (usuarioExistente != null || colaboradorExistente != null) ? 
                        "Este email já está cadastrado" : "Email disponível"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar email: {Email}", email);
                return Json(new { erro = "Erro ao verificar email", detalhes = ex.Message });
            }
        }

        [HttpGet("validar-cep")]
        public async Task<IActionResult> ValidarCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
            {
                return Json(new { valido = false, mensagem = "CEP não informado." });
            }

            cep = Regex.Replace(cep, @"[^\d]", "");

            using (var httpClient = new System.Net.Http.HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync($"https://viacep.com.br/ws/{cep}/json/");
                    return Content(response, "application/json");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao consultar CEP {Cep}", cep);
                    return Json(new { valido = false, mensagem = "Não foi possível consultar o CEP." });
                }
            }
        }



        [HttpGet("cadastro")]
        public IActionResult ExibirTelaCadastro()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Administrador"))
                {
                    return RedirectToAction("Inicio", "GerenciamentoDashboard");
                }
                else
                {
                    return Redirect("/usuario/pets/explorar");
                }
            }
            return View("~/Views/Autenticacao/Cadastro.cshtml", new Models.ViewModels.Usuario.UsuarioViewModel());
        }

        [HttpPost("cadastro")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarCadastro(CaotinhoAuMiau.Models.ViewModels.Usuario.UsuarioViewModel usuarioVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("~/Views/Autenticacao/Cadastro.cshtml", usuarioVM);
                }

                var usuario = usuarioVM.ConverterParaEntidade();
                
                string senhaOriginal = usuario.Senha;
                usuario.Senha = HashHelper.GerarHashSenha(usuario.Senha);
                usuario.DataCadastro = DateTime.Now;
                usuario.Ativo = true;

                _contexto.Usuarios.Add(usuario);
                await _contexto.SaveChangesAsync();

                TempData["Sucesso"] = "Cadastro realizado com sucesso! Por favor, faça o login.";
                
                return RedirectToAction("ExibirTelaLogin", "Autenticacao");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar cadastro para usuário {Email}", usuarioVM.Email);
                
                TempData["Erro"] = "Não foi possível completar seu cadastro. Por favor, tente novamente.";
                return View("~/Views/Autenticacao/Cadastro.cshtml", usuarioVM);
            }
        }
        


        #endregion
    }
} 