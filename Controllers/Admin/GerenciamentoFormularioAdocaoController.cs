using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using CaotinhoAuMiau.Models.ViewModels;
using CaotinhoAuMiau.Models.Enums;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;
using Microsoft.Extensions.Logging;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Authorize(Roles = "Administrador,Colaborador,Voluntário")]
    [Route("admin/formularios-adocao")]
    public class GerenciamentoFormularioAdocaoController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificationService _servicoNotificacao;
        private readonly ILogger<GerenciamentoFormularioAdocaoController> _logger;
        private readonly ContratoService _contratoServico;
        private readonly EmailService _emailService;

        public GerenciamentoFormularioAdocaoController(ApplicationDbContext contexto, NotificationService servicoNotificacao, ILogger<GerenciamentoFormularioAdocaoController> logger, ContratoService contratoServico, EmailService emailService)
        {
            _contexto = contexto;
            _servicoNotificacao = servicoNotificacao;
            _logger = logger;
            _contratoServico = contratoServico;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> ListarAsync(int pagina = 1, int itensPorPagina = 10, string filtroStatus = "", string filtroData = "", string pesquisa = "")
        {
            var adminId = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("ExibirTelaLogin", "Authentication");
            }
            
            var query = _contexto.FormulariosAdocao
                .Include(f => f.Pet)
                .Include(f => f.Usuario)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(filtroStatus))
            {
                query = query.Where(f => f.StatusEnum.ObterTexto().ToLower() == filtroStatus.ToLower());
            }
            
            if (!string.IsNullOrEmpty(filtroData))
            {
                var hoje = DateTime.Today;
                switch (filtroData.ToLower())
                {
                    case "hoje":
                        query = query.Where(f => f.DataEnvio.Date == hoje);
                        break;
                    case "7dias":
                        query = query.Where(f => f.DataEnvio.Date >= hoje.AddDays(-7));
                        break;
                    case "30dias":
                        query = query.Where(f => f.DataEnvio.Date >= hoje.AddDays(-30));
                        break;
                }
            }
            
            if (!string.IsNullOrEmpty(pesquisa))
            {
                pesquisa = pesquisa.ToLower();
                query = query.Where(f => 
                    (f.Usuario.Nome != null && f.Usuario.Nome.ToLower().Contains(pesquisa)) ||
                    (f.Usuario.Email != null && f.Usuario.Email.ToLower().Contains(pesquisa)) ||
                    (f.Usuario.Logradouro != null && f.Usuario.Logradouro.ToLower().Contains(pesquisa)) ||
                    (f.Usuario.Bairro != null && f.Usuario.Bairro.ToLower().Contains(pesquisa)) ||
                    (f.Usuario.Cidade != null && f.Usuario.Cidade.ToLower().Contains(pesquisa)) ||
                    (f.Usuario.Estado != null && f.Usuario.Estado.ToLower().Contains(pesquisa)) ||
                    (f.Pet.Nome != null && f.Pet.Nome.ToLower().Contains(pesquisa))
                );
            }
            
            var totalItens = await query.CountAsync();
            
            var formulariosPaginados = await query
                .OrderByDescending(f => f.DataEnvio)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            ViewBag.PaginaAtual = pagina;
            ViewBag.ItensPorPagina = itensPorPagina;
            ViewBag.TotalItens = totalItens;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalItens / (double)itensPorPagina);
            
            ViewBag.FiltroStatus = filtroStatus;
            ViewBag.FiltroData = filtroData;
            ViewBag.Pesquisa = pesquisa;

            return View("~/Views/Admin/GerenciamentoFormularios.cshtml", formulariosPaginados);
        }

        [HttpGet("detalhes/{id}")]
        public async Task<IActionResult> ObterDetalhesFormularioAsync(int id)
        {
            try
            {
            var formulario = await _contexto.FormulariosAdocao
                .Include(f => f.Pet)
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (formulario == null)
            {
                    return Json(new { sucesso = false, mensagem = "Formulário não encontrado." });
            }

                return Json(new {
                    sucesso = true,
                    formulario = formulario,
                    status = formulario.StatusEnum.ObterTexto(),
                    observacoesAdmin = formulario.ObservacaoAdminFormulario,
                    observacoesCancelamento = formulario.ObservacoesCancelamento
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erro ao obter detalhes do formulário {id}: {ex.Message}");
                
                return Json(new { sucesso = false, mensagem = $"Erro ao carregar detalhes do formulário: {ex.Message}" });
            }
        }

        [HttpPost("Aprovar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarFormularioAsync(int id, string observacao = "")
        {
            try
            {
                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .Include(f => f.Usuario)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (formulario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Formulário não encontrado." });
                }

                var pet = await _contexto.Pets.FindAsync(formulario.PetId);
                if (pet == null)
                {
                    return Json(new { sucesso = false, mensagem = "Pet não encontrado." });
                }

                if (pet.Status == StatusPet.EmProcesso)
                {
                    var formularioAprovado = await _contexto.FormulariosAdocao
                        .FirstOrDefaultAsync(f => f.PetId == pet.Id && f.StatusEnum == StatusFormulario.Aprovado && f.Id != id);
                        
                    if (formularioAprovado != null)
                    {
                        return Json(new { sucesso = false, mensagem = $"Este pet já está em processo de adoção por outro usuário. Status atual: {pet.Status}" });
                    }
                    
                }
                else if (pet.Status != StatusPet.Disponivel)
                {
                    return Json(new { sucesso = false, mensagem = $"Este pet não está disponível para adoção. Status atual: {pet.Status}" });
                }

                var outrosFormularios = await _contexto.FormulariosAdocao
                    .Where(f => f.PetId == pet.Id && f.Id != id && f.StatusEnum == StatusFormulario.Pendente)
                    .ToListAsync();
                    
                foreach (var outroForm in outrosFormularios)
                {
                    outroForm.StatusEnum = StatusFormulario.Negado;
                    outroForm.DataResposta = DateTime.Now;
                    outroForm.ObservacaoAdminFormulario = "Este formulário foi rejeitado pois outro candidato já foi aprovado para adotar este pet.";
                    
                    try
                    {
            await _servicoNotificacao.CriarNotificacaoAsync(
                            outroForm.UsuarioId.ToString(),
                            "Formulário de adoção rejeitado",
                            $"Seu formulário de adoção para o pet {pet.Nome} foi rejeitado porque outro candidato foi aprovado.",
                            "FormularioAdocao",
                            outroForm.Id.ToString()
                        );
                    }
                    catch (Exception notifEx)
                    {
                        _logger.LogError(notifEx, "Erro ao enviar notificação");
                    }
                }

                formulario.StatusEnum = StatusFormulario.Aprovado;
                formulario.DataResposta = DateTime.Now;
                
                if (!string.IsNullOrWhiteSpace(observacao))
                {
                    formulario.ObservacaoAdminFormulario = observacao;
                }

                pet.Status = StatusPet.EmProcesso;

                var agora = DateTime.Now;
                var adocao = new Adocao
                {
                    PetId = formulario.PetId,
                    UsuarioId = formulario.UsuarioId,
                    DataEnvio = formulario.DataEnvio,
                    DataResposta = agora,
                    Status = StatusAdocao.AguardandoAssinarContrato
                };

                _contexto.Adocoes.Add(adocao);

                await _contexto.SaveChangesAsync();

                try
                {
                    var resultadoContrato = await _contratoServico.GerarContratoAsync(adocao.Id);
                    if (resultadoContrato.sucesso)
                    {
                        _logger.LogInformation($"Contrato criado para adoção {adocao.Id}");
                    }
                    else
                    {
                        _logger.LogWarning($"Falha ao gerar contrato para adoção {adocao.Id}: {resultadoContrato.mensagem}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Erro ao gerar contrato para adoção {adocao.Id}");
                }

                if (formulario.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacaoAsync(
                        formulario.UsuarioId.ToString(),
                        "Formulário de adoção aprovado",
                        $"Seu formulário de adoção para o pet {pet.Nome} foi aprovado! Um contrato foi gerado e está aguardando sua assinatura.",
                        "Adocao",
                        formulario.Id.ToString()
                    );

                    try
                    {
                        await _emailService.EnviarEmailFormularioAprovadoAsync(formulario.Usuario, pet, formulario.Id);
                        _logger.LogInformation($"Email de formulário aprovado enviado para {formulario.Usuario.Email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Erro ao enviar email de formulário aprovado para {formulario.Usuario.Email}");
                    }
                }

                return Json(new { sucesso = true, mensagem = "Formulário aprovado com sucesso! Um contrato foi gerado e está aguardando assinatura." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao aprovar formulário: {ex.Message}" });
            }
        }

        [HttpPost("Rejeitar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarFormularioAsync(int id, string motivo)
        {
            try
            {
                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .Include(f => f.Usuario)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (formulario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Formulário não encontrado." });
                }

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    return Json(new { sucesso = false, mensagem = "É necessário fornecer um motivo para a rejeição." });
                }

                formulario.StatusEnum = StatusFormulario.Negado;
                formulario.DataResposta = DateTime.Now;
                formulario.ObservacaoAdminFormulario = motivo;

                var outrosFormulariosAprovados = await _contexto.FormulariosAdocao
                    .Where(f => f.PetId == formulario.PetId && f.StatusEnum == StatusFormulario.Aprovado && f.Id != formulario.Id)
                    .AnyAsync();

                if (!outrosFormulariosAprovados && formulario.Pet != null)
                {
                    formulario.Pet.Status = StatusPet.Disponivel;
                    _contexto.Pets.Update(formulario.Pet);
                }

                if (formulario.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacaoAsync(
                        formulario.UsuarioId.ToString(),
                        "Formulário de adoção rejeitado",
                        $"Seu formulário de adoção para o pet {formulario.Pet?.Nome} foi rejeitado. Motivo: {motivo}",
                        "Adocao",
                        formulario.Id.ToString()
                    );
                }

                await _contexto.SaveChangesAsync();

                return Json(new { sucesso = true, mensagem = "Formulário rejeitado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao rejeitar formulário: {ex.Message}" });
            }
        }

        [HttpPost("solicitar-informacoes/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarInformacoesAdicionaisAsync(int id, [FromBody] string observacao)
        {
            try
            {
                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .Include(f => f.Usuario)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (formulario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Formulário não encontrado" });
                }

                formulario.StatusEnum = StatusFormulario.EmAnalise;
                formulario.ObservacaoAdminFormulario = observacao;
                formulario.DataResposta = DateTime.Now;

                if (formulario.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacaoAsync(
                        formulario.UsuarioId.ToString(),
                        "Informações adicionais solicitadas",
                        $"Por favor, forneça informações adicionais para seu formulário de adoção do pet {formulario.Pet?.Nome}",
                        "FormularioAdocao",
                        formulario.Id.ToString()
                    );
                }

                await _contexto.SaveChangesAsync();

                return Json(new { sucesso = true, mensagem = "Solicitação de informações enviada com sucesso" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao solicitar informações: {ex.Message}" });
            }
        }
    }
}
