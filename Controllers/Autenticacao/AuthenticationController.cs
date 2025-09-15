using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Models.ViewModels.Comuns;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using CaotinhoAuMiau.Utils;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models.Enums;

namespace CaotinhoAuMiau.Controllers.Autenticacao
{
    [Route("autenticacao")]
    public class AuthenticationController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IColaboradorService _colaboradorService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly ApplicationDbContext _contexto;
        private readonly IAuditoriaService _auditoriaService;

        public AuthenticationController(IUsuarioService usuarioService, IColaboradorService colaboradorService,
            ILogger<AuthenticationController> logger, ApplicationDbContext contexto, IAuditoriaService auditoriaService)
        {
            _usuarioService = usuarioService;
            _colaboradorService = colaboradorService;
            _logger = logger;
            _contexto = contexto;
            _auditoriaService = auditoriaService;
        }

        [HttpGet("login")]
        public IActionResult ExibirTelaLogin(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Autenticacao/Login.cshtml");
        }

        [HttpPost("login")]
        public async Task<IActionResult> EfetuarLoginAsync(LoginViewModel modelo, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Autenticacao/Login.cshtml", modelo);
            }

            try
            {
                _logger.LogInformation("Tentativa de login - Email: {Email}", modelo.Email);

                // Primeiro checamos se o email pertence a um colaborador
                var colaboradorExiste = await _contexto.Colaboradores
                    .AnyAsync(c => c.Email == modelo.Email && c.Ativo);

                if (colaboradorExiste)
                {
                    // Só registramos auditoria para tentativas de login da equipe
                    await _auditoriaService.RegistrarAcaoAsync(
                        "Tentativa_Login",
                        $"Tentativa de login de colaborador: {modelo.Email}",
                        "Autenticação"
                    );
                }

                // Pegamos os detalhes do colaborador para validação
                var colaboradorDetalhes = await _contexto.Colaboradores
                    .Where(c => c.Email == modelo.Email)
                    .Select(c => new { c.Id, c.Email, c.Cargo, CargoInt = (int)c.Cargo, c.Ativo, HasSenha = !string.IsNullOrEmpty(c.Senha) })
                    .FirstOrDefaultAsync();

                _logger.LogInformation("PASSO 1 - Colaborador existe na base: {Existe}", colaboradorDetalhes != null);
                if (colaboradorDetalhes != null)
                {
                    _logger.LogInformation("PASSO 2 - Dados do colaborador: Id={Id}, Email={Email}, CargoEnum={Cargo}, CargoInt={CargoInt}, Ativo={Ativo}, TemSenha={TemSenha}",
                        colaboradorDetalhes.Id, colaboradorDetalhes.Email, colaboradorDetalhes.Cargo, colaboradorDetalhes.CargoInt, colaboradorDetalhes.Ativo, colaboradorDetalhes.HasSenha);
                }

                _logger.LogInformation("PASSO 3 - Chamando ColaboradorService.AutenticarAsync...");
                var colaborador = await _colaboradorService.AutenticarAsync(modelo.Email, modelo.Senha);
                _logger.LogInformation("PASSO 4 - Resultado autenticação: {Resultado}", colaborador != null ? "SUCESSO" : "FALHOU");

                if (colaborador != null)
                {
                    _logger.LogInformation("PASSO 5 - Dados do colaborador autenticado: Id={Id}, Email={Email}, Cargo={Cargo}, CargoInt={CargoInt}",
                        colaborador.Id, colaborador.Email, colaborador.Cargo, (int)colaborador.Cargo);
                }
                
                if (colaborador != null)
                {
                    _logger.LogInformation("PASSO 6 - Iniciando processo de criação de claims...");

                    var roleString = colaborador.Cargo.ToString(); // "Administrador", "Colaborador", etc.
                    _logger.LogInformation("PASSO 7 - Convertendo cargo para string: Enum={CargoEnum} (valor={CargoInt}) -> String='{RoleString}'",
                        colaborador.Cargo, (int)colaborador.Cargo, roleString);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, colaborador.Id.ToString()),
                        new Claim(ClaimTypes.Name, colaborador.Nome),
                        new Claim(ClaimTypes.Email, colaborador.Email),
                        new Claim(ClaimTypes.Role, roleString),
                        new Claim("Cargo", roleString)
                    };

                    _logger.LogInformation("PASSO 8 - Claims criadas: {Claims}",
                        string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));

                    var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var propriedadesAutenticacao = new AuthenticationProperties
                    {
                        IsPersistent = modelo.ContinuarConectado,
                        ExpiresUtc = modelo.ContinuarConectado ? DateTimeOffset.UtcNow.AddDays(30) : null // Sessão temporária se não marcou o checkbox
                    };

                    _logger.LogInformation("PASSO 9 - Fazendo SignIn...");
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identidade),
                        propriedadesAutenticacao);

                    _logger.LogInformation("PASSO 10 - SignIn concluído! Redirecionando para /admin/dashboard");

                    // Registra que um membro da equipe fez login com sucesso
                    await _auditoriaService.RegistrarLoginAsync(
                        colaborador.Email,
                        true,
                        null,
                        colaborador.Nome,
                        colaborador.Cargo.ToString()
                    );

                    // Colaboradores vão direto pro painel administrativo
                    return Redirect("/admin/dashboard");
                }
                
                // Bloqueia usuários que foram desativados
                var usuarioInativo = await _contexto.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == modelo.Email && !u.Ativo);

                if (usuarioInativo != null)
                {
                    _logger.LogWarning("Tentativa de login de usuário inativo: {Email}", modelo.Email);
                    TempData["Erro"] = "Sua conta foi desativada por violações das políticas da plataforma. Para dúvidas, entre em contato pelo email contato@caotinhoaumiau.com.br";
                    return View("~/Views/Autenticacao/Login.cshtml", modelo);
                }

                var usuario = await _usuarioService.AutenticarAsync(modelo.Email, modelo.Senha);
                _logger.LogInformation("Resultado autenticação usuário para {Email}: {Resultado}", modelo.Email, usuario != null ? "Sucesso" : "Falhou");

                if (usuario != null)
                {
                    _logger.LogInformation("Login de usuário realizado - Email: {Email}", usuario.Email);
                    
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Nome),
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim(ClaimTypes.Role, "Usuario"),
                        new Claim("Cargo", "Usuario")
                    };

                    var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var propriedadesAutenticacao = new AuthenticationProperties
                    {
                        IsPersistent = modelo.ContinuarConectado,
                        ExpiresUtc = modelo.ContinuarConectado ? DateTimeOffset.UtcNow.AddDays(30) : null // Sessão temporária se não marcou o checkbox
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identidade),
                        propriedadesAutenticacao);

                    await _usuarioService.AtualizarUltimoAcessoUsuarioAsync(usuario);

                    return RedirectToLocal(returnUrl) ?? Redirect("/usuario/pets/explorar");
                }

                // Só logamos falhas de login da equipe por segurança
                if (colaboradorExiste)
                {
                    await _auditoriaService.RegistrarLoginAsync(
                        modelo.Email,
                        false,
                        "Credenciais inválidas"
                    );
                }

                TempData["Erro"] = "Email ou senha incorretos.";
                return View("~/Views/Autenticacao/Login.cshtml", modelo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante tentativa de login para email: {Email}. Erro: {Erro}, StackTrace: {StackTrace}",
                    modelo.Email, ex.Message, ex.StackTrace);
                TempData["Erro"] = $"Erro interno: {ex.Message}";
                return View("~/Views/Autenticacao/Login.cshtml", modelo);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            // Registra quando alguém da equipe sai do sistema
            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var nome = User.FindFirst(ClaimTypes.Name)?.Value;
                var cargo = User.FindFirst("Cargo")?.Value;

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(cargo))
                {
                    await _auditoriaService.RegistrarAcaoAsync(
                        "Logout",
                        $"Logout de colaborador: {nome} ({cargo})",
                        "Autenticação"
                    );
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("cadastro")]
        public IActionResult ExibirTelaCadastro()
        {
            return View("~/Views/Autenticacao/Cadastro.cshtml");
        }

        [HttpPost("cadastro")]
        public async Task<IActionResult> EfetuarCadastroAsync(UsuarioViewModel modelo)
        {
            _logger.LogInformation($"Iniciando cadastro para email: {modelo?.Email}");
            
            // Limpa as máscaras que vem do frontend
            if (!string.IsNullOrEmpty(modelo.CPF))
            {
                modelo.CPF = modelo.CPF.Replace(".", "").Replace("-", "");
            }
            
            if (!string.IsNullOrEmpty(modelo.CEP))
            {
                modelo.CEP = modelo.CEP.Replace("-", "");
            }
            
            if (!string.IsNullOrEmpty(modelo.Telefone))
            {
                modelo.Telefone = modelo.Telefone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
            }
            
            // Campo complemento pode ser vazio mas não null
            if (string.IsNullOrEmpty(modelo.Complemento))
            {
                modelo.Complemento = "";
            }
            
            // Remove erros de validação relacionados às máscaras
            ModelState.Remove("CPF");
            ModelState.Remove("CEP");
            ModelState.Remove("Telefone");
            ModelState.Remove("Complemento");
            
            if (!ModelState.IsValid)
            {
                // Ajuda a debugar problemas de validação no cadastro
                foreach (var error in ModelState)
                {
                    if (error.Value.Errors.Count > 0)
                    {
                        _logger.LogWarning($"Erro de validação no campo {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
                return View("~/Views/Autenticacao/Cadastro.cshtml", modelo);
            }

            if (!Validators.ValidarEmail(modelo.Email))
            {
                ModelState.AddModelError("Email", "Formato de e-mail inválido.");
                return View("~/Views/Autenticacao/Cadastro.cshtml", modelo);
            }


            if (string.IsNullOrWhiteSpace(modelo.Senha) || modelo.Senha.Length < 6)
            {
                ModelState.AddModelError("Senha", "A senha deve ter pelo menos 6 caracteres.");
                return View("~/Views/Autenticacao/Cadastro.cshtml", modelo);
            }

            try
            {
                if (await _usuarioService.EmailExisteAsync(modelo.Email))
                {
                    ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
                    return View("~/Views/Autenticacao/Cadastro.cshtml", modelo);
                }

                if (await _usuarioService.CPFExisteAsync(modelo.CPF))
                {
                    ModelState.AddModelError("CPF", "Este CPF já está cadastrado.");
                    return View("~/Views/Autenticacao/Cadastro.cshtml", modelo);
                }

                var usuario = new Models.Usuario
                {
                    Nome = modelo.Nome,
                    Email = modelo.Email,
                    CPF = modelo.CPF,
                    Telefone = modelo.Telefone,
                    Senha = modelo.Senha,
                    DataNascimento = modelo.DataNascimento,
                    CEP = modelo.CEP,
                    Logradouro = modelo.Logradouro,
                    Numero = modelo.Numero,
                    Complemento = modelo.Complemento,
                    Bairro = modelo.Bairro,
                    Cidade = modelo.Cidade,
                    Estado = modelo.Estado
                };

                await _usuarioService.RegistrarUsuarioAsync(usuario);
                TempData["Sucesso"] = "Cadastro realizado com sucesso! Faça login para continuar.";
                return RedirectToAction("ExibirTelaLogin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante cadastro para email: {Email}", modelo.Email);
                TempData["Erro"] = "Erro interno. Tente novamente.";
                return View("~/Views/Autenticacao/Cadastro.cshtml", modelo);
            }
        }

        private IActionResult? RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return null;
        }



        private static bool ValidarCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = System.Text.RegularExpressions.Regex.Replace(cpf, @"[^\d]", "");

            if (cpf.Length != 11)
                return false;

            if (cpf.All(c => c == cpf[0]))
                return false;

            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (10 - i);
            }
            int primeiroDigito = 11 - (soma % 11);
            if (primeiroDigito > 9) primeiroDigito = 0;

            if (int.Parse(cpf[9].ToString()) != primeiroDigito)
                return false;

            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (11 - i);
            }
            int segundoDigito = 11 - (soma % 11);
            if (segundoDigito > 9) segundoDigito = 0;

            return int.Parse(cpf[10].ToString()) == segundoDigito;
        }
    }
}