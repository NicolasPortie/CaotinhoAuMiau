using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using CaotinhoAuMiau.Services;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Authorize(Roles = "Administrador")]
    [Route("Admin/Email")]
    public class GerenciamentoEmailController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly EmailService _emailService;
        private readonly ILogger<GerenciamentoEmailController> _logger;
        private readonly IAuditoriaService _auditoriaService;

        public GerenciamentoEmailController(ApplicationDbContext contexto, EmailService emailService, ILogger<GerenciamentoEmailController> logger, IAuditoriaService auditoriaService)
        {
            _contexto = contexto;
            _emailService = emailService;
            _logger = logger;
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Configuracoes()
        {
            var viewModel = await PrepararViewModelConfiguracoes();
            return View("~/Views/Admin/GerenciamentoEmailConfiguracoes.cshtml", viewModel);
        }

        [HttpPost("salvar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarConfiguracao(GerenciamentoEmailViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Erro"] = "Por favor, corrija os erros no formulário.";
                    viewModel = await PrepararViewModelConfiguracoes(viewModel.Configuracao);
                    return View("~/Views/Admin/GerenciamentoEmailConfiguracoes.cshtml", viewModel);
                }

                var configuracoesAnteriores = await _contexto.ConfiguracoesEmail
                    .Where(c => c.Ativo)
                    .ToListAsync();

                foreach (var config in configuracoesAnteriores)
                {
                    config.Ativo = false;
                }

                viewModel.Configuracao.Ativo = true;
                viewModel.Configuracao.DataCriacao = DateTime.Now;
                _contexto.ConfiguracoesEmail.Add(viewModel.Configuracao);

                await _contexto.SaveChangesAsync();

                TempData["Sucesso"] = "Configuração de email salva com sucesso!";

                await _auditoriaService.RegistrarAcaoAsync("SalvarConfiguracaoEmail", $"Configuração de email alterada - Servidor: {viewModel.Configuracao.ServidorSmtp}");

                return RedirectToAction("Configuracoes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar configuração de email");
                TempData["Erro"] = "Erro ao salvar configuração de email.";
                viewModel = await PrepararViewModelConfiguracoes(viewModel.Configuracao);
                return View("~/Views/Admin/GerenciamentoEmailConfiguracoes.cshtml", viewModel);
            }
        }

        [HttpPost("testar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestarConfiguracao()
        {
            try
            {
                var configuracao = await _contexto.ConfiguracoesEmail
                    .Where(c => c.Ativo)
                    .FirstOrDefaultAsync();

                if (configuracao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Nenhuma configuração ativa encontrada." });
                }

                var emailTeste = new
                {
                    Para = configuracao.EmailRemetente,
                    Assunto = "Teste de Configuração - CaotinhoAuMiau",
                    Corpo = $"<h3>Teste de Configuração de Email</h3><p>Se você está recebendo este email, a configuração está funcionando corretamente!</p><p>Data do teste: {DateTime.Now:dd/MM/yyyy HH:mm}</p>"
                };

                var resultado = await _emailService.EnviarEmailAsync(
                    emailTeste.Para,
                    emailTeste.Assunto,
                    emailTeste.Corpo
                );

                if (resultado)
                {
                    await _auditoriaService.RegistrarAcaoAsync("TesteEmail", $"Teste de email enviado com sucesso para {emailTeste.Para}");
                    return Json(new { sucesso = true, mensagem = "Email de teste enviado com sucesso!" });
                }
                else
                {
                    return Json(new { sucesso = false, mensagem = "Falha ao enviar email de teste." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar configuração de email");
                return Json(new { sucesso = false, mensagem = $"Erro ao testar configuração: {ex.Message}" });
            }
        }

        [HttpPost("testar-direto")]
        public async Task<IActionResult> TestarConfiguracaoDireto([FromBody] TesteDiretoEmailRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.EmailDestino))
                {
                    return Json(new { sucesso = false, mensagem = "Email de destino é obrigatório." });
                }

                var emailTeste = new
                {
                    Para = request.EmailDestino,
                    Assunto = "Teste Direto de Configuração - CaotinhoAuMiau",
                    Corpo = $"<h3>Teste Direto de Email</h3><p>Este é um email de teste enviado diretamente com as configurações atuais.</p><p>Data: {DateTime.Now:dd/MM/yyyy HH:mm}</p>"
                };

                var resultado = await _emailService.EnviarEmailAsync(
                    emailTeste.Para,
                    emailTeste.Assunto,
                    emailTeste.Corpo
                );

                if (resultado)
                {
                    await _auditoriaService.RegistrarAcaoAsync("TesteEmailDireto", $"Teste direto de email enviado para {emailTeste.Para}");
                    return Json(new { sucesso = true, mensagem = $"Email enviado com sucesso para {request.EmailDestino}!" });
                }
                else
                {
                    return Json(new { sucesso = false, mensagem = "Falha ao enviar email." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no teste direto de email para {Email}", request?.EmailDestino);
                return Json(new { sucesso = false, mensagem = $"Erro: {ex.Message}" });
            }
        }

        private async Task<GerenciamentoEmailViewModel> PrepararViewModelConfiguracoes(ConfiguracaoEmail? configuracaoAtual = null)
        {
            var configuracao = configuracaoAtual ?? await _contexto.ConfiguracoesEmail
                .Where(c => c.Ativo)
                .FirstOrDefaultAsync() ?? new ConfiguracaoEmail();

            return new GerenciamentoEmailViewModel
            {
                Configuracao = configuracao
            };
        }
    }

    public class TesteDiretoEmailRequest
    {
        public string EmailDestino { get; set; } = string.Empty;
    }
}