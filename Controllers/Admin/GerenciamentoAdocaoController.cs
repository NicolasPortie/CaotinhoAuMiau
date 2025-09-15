using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using CaotinhoAuMiau.Models.Enums;
using static CaotinhoAuMiau.Models.Enums.StatusFormulario;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Constants;
using Microsoft.Extensions.Logging;

public class CancelarAdocaoRequest
{
    public string Motivo { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
}

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Authorize(Roles = "Administrador")]
    [Route("admin/adocoes")]
    public class GerenciamentoAdocaoController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificationService _servicoNotificacao;
        private readonly HistoricoAdocaoService _historicoServico;
        private readonly ILogger<GerenciamentoAdocaoController> _logger;
        private readonly ContratoService _contratoServico;
        private readonly IAuditoriaService _auditoriaService;
        private readonly PdfService _pdfService;
        private readonly EmailService _emailService;

        public GerenciamentoAdocaoController(ApplicationDbContext contexto, NotificationService servicoNotificacao, HistoricoAdocaoService historicoServico, ILogger<GerenciamentoAdocaoController> logger, ContratoService contratoServico, IAuditoriaService auditoriaService, PdfService pdfService, EmailService emailService)
        {
            _contexto = contexto;
            _servicoNotificacao = servicoNotificacao;
            _historicoServico = historicoServico;
            _logger = logger;
            _contratoServico = contratoServico;
            _auditoriaService = auditoriaService;
            _pdfService = pdfService;
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
            
            var query = _contexto.Adocoes
                .Include(a => a.Pet)
                .Include(a => a.Usuario)
                .Include(a => a.Contrato)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(filtroStatus))
            {
                if (Enum.TryParse<StatusAdocao>(filtroStatus, out var status))
                {
                    query = query.Where(a => a.Status == status);
                }
            }
            
            if (!string.IsNullOrEmpty(filtroData))
            {
                var hoje = DateTime.Today;
                switch (filtroData.ToLower())
                {
                    case "hoje":
                        query = query.Where(a => a.DataEnvio.Date == hoje);
                        break;
                    case "7dias":
                        query = query.Where(a => a.DataEnvio.Date >= hoje.AddDays(-7));
                        break;
                    case "30dias":
                        query = query.Where(a => a.DataEnvio.Date >= hoje.AddDays(-30));
                        break;
                }
            }
            
            if (!string.IsNullOrEmpty(pesquisa))
            {
                pesquisa = pesquisa.ToLower();
                query = query.Where(a => 
                    (a.Usuario != null && a.Usuario.Nome != null && a.Usuario.Nome.ToLower().Contains(pesquisa)) ||
                    (a.Usuario != null && a.Usuario.Email != null && a.Usuario.Email.ToLower().Contains(pesquisa)) ||
                    (a.Pet != null && a.Pet.Nome != null && a.Pet.Nome.ToLower().Contains(pesquisa)) ||
                    a.Status.ToString().ToLower().Contains(pesquisa)
                );
            }
            
            var totalItens = await query.CountAsync();
            
            var adocoesPaginadas = await query
                .OrderByDescending(a => a.DataEnvio)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            var adocoesViewModel = adocoesPaginadas.Select(a => {
                var pet = a.Pet;
                string idadeFormatada = "Idade não informada";
                if (pet != null)
                {
                    if (pet.Anos > 0)
                    {
                        idadeFormatada = $"{pet.Anos} {(pet.Anos == 1 ? "ano" : "anos")}";
                        if (pet.Meses > 0)
                        {
                            idadeFormatada += $" e {pet.Meses} {(pet.Meses == 1 ? "mês" : "meses")}";
                        }
                    }
                    else if (pet.Meses > 0)
                    {
                        idadeFormatada = $"{pet.Meses} {(pet.Meses == 1 ? "mês" : "meses")}";
                    }
                    else
                    {
                        idadeFormatada = "Recém-nascido";
                    }
                }

                return new AdocaoAdminSummaryViewModel
                {
                    Id = a.Id,
                    Status = a.Status.ObterTexto(),
                    DataEnvio = a.DataEnvio,
                    DataResposta = a.DataResposta,
                    DataAssinatura = a.Contrato?.DataAssinatura,
                    DataFinalizacao = a.DataFinalizacao,
                    DataCriacaoContrato = a.Contrato?.DataCriacao,
                    PetId = a.PetId,
                    PetNome = pet?.Nome ?? "Não informado",
                    PetEspecie = pet?.Especie ?? Especie.Cao,
                    PetRaca = $"{(pet?.Raca ?? "SRD")} ({idadeFormatada})",
                    PetImagem = pet?.NomeArquivoImagem,
                    UsuarioId = a.UsuarioId,
                    UsuarioNome = a.Usuario?.Nome ?? "Não informado",
                    UsuarioEmail = a.Usuario?.Email ?? "Não informado",
                    UsuarioTelefone = a.Usuario?.Telefone,
                    TemContrato = a.ContratoId.HasValue,
                    ContratoId = a.ContratoId,
                    ContratoAssinado = a.ContratoAssinado
                };
            }).ToList();

            var todasAdocoes = await _contexto.Adocoes.ToListAsync();
            var formulariosPendentes = await _contexto.FormulariosAdocao.CountAsync(f => f.StatusEnum == StatusFormulario.Pendente);
            var stats = new AdocaoStatisticsViewModel
            {
                TotalAdocoes = totalItens,
                ContratoPendente = todasAdocoes.Count(a => a.Status == StatusAdocao.AguardandoAssinarContrato),
                ContratoAssinado = todasAdocoes.Count(a => a.Status == StatusAdocao.ContratoAssinado),
                AguardandoBusca = todasAdocoes.Count(a => a.Status == StatusAdocao.AguardandoBuscar),
                AdocoesFinalizadas = todasAdocoes.Count(a => a.Status == StatusAdocao.Finalizado),
                AdocoesRejeitadas = todasAdocoes.Count(a => a.Status.EstaCancelada())
            };

            var viewModel = new GerenciamentoAdocaoViewModel
            {
                Adocoes = adocoesViewModel,
                Statistics = stats,
                FilterOptions = AdocaoFilterOptionsViewModel.Create(),
                Pagination = new PaginationViewModel
                {
                    PaginaAtual = pagina,
                    TotalPaginas = (int)Math.Ceiling(totalItens / (double)itensPorPagina),
                    TotalItens = totalItens,
                    ItensPorPagina = itensPorPagina
                },
                FiltroStatus = filtroStatus,
                FiltroData = filtroData,
                Pesquisa = pesquisa,
                OrdenarPor = "DataEnvio",
                DirecaoOrdem = "Desc"
            };

            if (!string.IsNullOrEmpty(filtroStatus) || !string.IsNullOrEmpty(filtroData) || !string.IsNullOrEmpty(pesquisa) || pagina > 1)
            {
                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.FILTRAR_DADOS,
                    "Filtros aplicados na listagem de adoções",
                    LogConstants.Categorias.ADOCAO,
                    detalhesAdicionais: $"Status: {filtroStatus}, Data: {filtroData}, Pesquisa: {pesquisa}, Pagina: {pagina}"
                );
            }

            return View("~/Views/Admin/GerenciamentoAdocoes.cshtml", viewModel);
        }

        [HttpPost("aprovar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarAdocaoAsync(int id)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada." });
                }
                
                if (adocao.Status == StatusAdocao.AguardandoAssinarContrato || adocao.Status.EstaCancelada())
                {
                    return Json(new { sucesso = false, mensagem = $"Esta adoção já foi {adocao.Status.ToString().ToLower()}." });
                }
                
                adocao.Status = StatusAdocao.AguardandoAssinarContrato;
                adocao.DataResposta = DateTime.Now;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = StatusPet.EmProcesso;
                }
                
                await _contexto.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.APROVAR_ADOCAO,
                    $"Adoção aprovada: Pet {pet?.Nome} para {adocao.Usuario?.Nome}",
                    LogConstants.Categorias.ADOCAO,
                    LogConstants.EntidadesAfetadas.ADOCAO,
                    adocao.Id,
                    LogConstants.NiveisSeveridade.INFO,
                    $"Pet ID: {adocao.PetId}, Usuário ID: {adocao.UsuarioId}"
                );
                
                return Json(new { sucesso = true, mensagem = "Adoção aprovada com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao aprovar adoção: {ex.Message}" });
            }
        }

        [HttpGet("detalhes/{id}")]
        public async Task<IActionResult> DetalhesAsync(int id)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Pet)
                    .Include(a => a.Usuario)
                    .Include(a => a.Contrato)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada." });
                }

                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.VISUALIZAR_DETALHES,
                    $"Detalhes da adoção visualizados - Pet: {adocao.Pet?.Nome}",
                    LogConstants.Categorias.ADOCAO,
                    LogConstants.EntidadesAfetadas.ADOCAO,
                    adocao.Id
                );

                var formulario = await _contexto.FormulariosAdocao
                    .FirstOrDefaultAsync(f => f.PetId == adocao.PetId && f.UsuarioId == adocao.UsuarioId);


                var dados = new
                {
                    id = adocao.Id,
                    status = adocao.Status.ObterTexto(),
                    dataEnvio = adocao.DataEnvio.ToString("dd/MM/yyyy HH:mm"),
                    dataResposta = adocao.DataResposta?.ToString("dd/MM/yyyy HH:mm"),
                    dataFinalizacao = adocao.DataFinalizacao?.ToString("dd/MM/yyyy HH:mm"),
                    contratoAssinado = adocao.ContratoAssinado,
                    contratoId = adocao.ContratoId,
                    pet = adocao.Pet != null ? new
                    {
                        id = adocao.Pet.Id,
                        nome = adocao.Pet.Nome,
                        especie = adocao.Pet.Especie.ObterTexto(),
                        raca = adocao.Pet.Raca,
                        porte = adocao.Pet.Porte,
                        sexo = adocao.Pet.Sexo.ObterValorMembroEnum(),
                        idade = $"{adocao.Pet.Anos} anos" + (adocao.Pet.Meses > 0 ? $" e {adocao.Pet.Meses} meses" : ""),
                        imagem = adocao.Pet.NomeArquivoImagem
                    } : null,
                    usuario = adocao.Usuario != null ? new
                    {
                        id = adocao.Usuario.Id,
                        nome = adocao.Usuario.Nome,
                        email = adocao.Usuario.Email,
                        telefone = adocao.Usuario.Telefone,
                        cpf = adocao.Usuario.CPF
                    } : null,
                    contrato = new
                    {
                        id = adocao.Contrato?.Id,
                        dataAssinatura = adocao.Contrato?.DataAssinatura?.ToString("dd/MM/yyyy HH:mm"),
                        dataCriacao = adocao.Contrato?.DataCriacao.ToString("dd/MM/yyyy HH:mm"),
                        temContrato = adocao.Contrato != null,
                        estaAssinado = adocao.ContratoAssinado == true,
                        caminhoArquivoPdf = adocao.Contrato?.CaminhoArquivoPdf
                    },
                    formulario = formulario != null ? new
                    {
                        rendaMensal = formulario.RendaMensal,
                        numeroMoradores = formulario.NumeroMoradores,
                        descricaoMoradia = formulario.DescricaoMoradia,
                        espacoAdequado = formulario.EspacoAdequado,
                        tempoDisponivel = formulario.TempoDisponivel,
                        tevePetAnterior = formulario.TevePetAnterior,
                        experienciaAnterior = formulario.ExperienciaAnterior,
                        motivacaoAdocao = formulario.MotivacaoAdocao,
                        condicoesFinanceiras = formulario.CondicoesFinanceiras,
                        planejamentoViagens = formulario.PlanejamentoViagens,
                        observacaoAdmin = formulario.ObservacaoAdminFormulario
                    } : null
                };

                return Json(new { sucesso = true, dados = dados });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes da adoção {AdocaoId}", id);
                return Json(new { sucesso = false, mensagem = "Erro interno do servidor." });
            }
        }

        [HttpPost("rejeitar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarAdocaoAsync(int id, string motivo)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada." });
                }
                
                if (string.IsNullOrWhiteSpace(motivo))
                {
                    return Json(new { sucesso = false, mensagem = "É necessário fornecer um motivo para a rejeição." });
                }
                
                if (adocao.Status == StatusAdocao.AguardandoAssinarContrato || adocao.Status.EstaCancelada())
                {
                    return Json(new { sucesso = false, mensagem = $"Esta adoção já foi {adocao.Status.ToString().ToLower()}." });
                }
                
                adocao.Status = StatusAdocao.CanceladoPeloCaotinho;
                adocao.DataResposta = DateTime.Now;
                adocao.ObservacoesCancelamento = motivo;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = StatusPet.Disponivel;
                }
                
                await _contexto.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.REJEITAR_ADOCAO,
                    $"Adoção rejeitada - Pet: {pet?.Nome} (ID: {adocao.PetId})",
                    LogConstants.Categorias.ADOCAO,
                    LogConstants.EntidadesAfetadas.ADOCAO,
                    adocao.Id,
                    LogConstants.NiveisSeveridade.WARNING,
                    detalhesAdicionais: $"Motivo: {motivo}"
                );

                return Json(new { sucesso = true, mensagem = "Adoção rejeitada com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao rejeitar adoção: {ex.Message}" });
            }
        }

        [HttpPost("finalizar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarAdocaoAsync(int id)
        {
            _logger.LogInformation("Método FinalizarAdocaoAsync chamado para ID: {AdocaoId}", id);
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada." });
                }

                if (adocao.Status == StatusAdocao.Finalizado)
                {
                    return Json(new { sucesso = false, mensagem = "Esta adoção já está finalizada." });
                }

                if (adocao.Status.EstaCancelada())
                {
                    return Json(new { sucesso = false, mensagem = "Não é possível finalizar uma adoção cancelada." });
                }

                adocao.Status = StatusAdocao.Finalizado;
                adocao.DataFinalizacao = DateTime.Now;

                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = StatusPet.Adotado;
                }

                await _contexto.SaveChangesAsync();
                _logger.LogInformation("Adoção {AdocaoId} salva no banco. Registrando log de auditoria...", id);

                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.CONFIRMAR_RETIRADA,
                    $"Adoção finalizada - Pet: {pet?.Nome} (ID: {adocao.PetId}) adotado com sucesso",
                    LogConstants.Categorias.ADOCAO,
                    LogConstants.EntidadesAfetadas.ADOCAO,
                    adocao.Id,
                    detalhesAdicionais: $"Data finalização: {DateTime.Now:dd/MM/yyyy HH:mm}"
                );

                _logger.LogInformation("Log de auditoria registrado para adoção {AdocaoId}", id);

                try
                {
                    var emailEnviado = await _emailService.EnviarEmailFinalizacaoAdocaoAsync(id);
                    if (emailEnviado)
                    {
                        _logger.LogInformation("Email de finalização enviado com sucesso para adoção {AdocaoId}", id);
                    }
                    else
                    {
                        _logger.LogWarning("Falha ao enviar email de finalização para adoção {AdocaoId}", id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar email de finalização para adoção {AdocaoId}", id);
                }

                if (adocao.Usuario != null && pet != null)
                {
                    try
                    {
                        await _servicoNotificacao.CriarNotificacaoAsync(
                            adocao.Usuario.Id.ToString(),
                            "Adoção Finalizada!",
                            $"Parabéns! A adoção do {pet.Nome} foi finalizada com sucesso. Desejamos muito amor e felicidade juntos!",
                            "sucesso",
                            $"adocao_finalizada_{adocao.Id}"
                        );
                        _logger.LogInformation("Notificação de finalização criada para usuário {UsuarioId} da adoção {AdocaoId}", adocao.Usuario.Id, id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao criar notificação de finalização para adoção {AdocaoId}", id);
                    }
                }

                return Json(new { sucesso = true, mensagem = "Adoção finalizada com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao finalizar adoção: {ex.Message}" });
            }
        }

        [HttpGet("VisualizarContrato/{id}")]
        public async Task<IActionResult> VisualizarContratoAsync(int id)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Contrato)
                    .Include(a => a.Pet)
                    .Include(a => a.Usuario)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (adocao == null)
                {
                    return NotFound("Adoção não encontrada.");
                }

                if (adocao.Contrato == null || !adocao.ContratoAssinado)
                {
                    return NotFound("Contrato não disponível ou não assinado.");
                }

                string? caminhoArquivo = null;

                // Reaproveita o PDF que já foi gerado anteriormente
                if (!string.IsNullOrEmpty(adocao.Contrato.CaminhoArquivoPdf))
                {
                        var caminhoRelativo = adocao.Contrato.CaminhoArquivoPdf.Replace("\\", "/").TrimStart('/');
                    var caminhoExistente = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", caminhoRelativo);

                    _logger.LogInformation("Procurando PDF em: {CaminhoExistente}, Caminho original: {CaminhoOriginal}",
                        caminhoExistente, adocao.Contrato.CaminhoArquivoPdf);

                    if (System.IO.File.Exists(caminhoExistente))
                    {
                        caminhoArquivo = caminhoExistente;
                    }
                }

                if (caminhoArquivo == null)
                {
                    _logger.LogInformation("PDF não encontrado, criando novo arquivo para adoção {AdocaoId}", id);

                    var resultado = await _pdfService.GerarPdfContratoAsync(adocao.Contrato);

                    if (resultado.sucesso && !string.IsNullOrEmpty(resultado.caminhoArquivo))
                    {
                            adocao.Contrato.CaminhoArquivoPdf = resultado.caminhoArquivo;
                        await _contexto.SaveChangesAsync();

                        caminhoArquivo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", resultado.caminhoArquivo);
                    }
                }

                if (caminhoArquivo == null || !System.IO.File.Exists(caminhoArquivo))
                {
                    _logger.LogWarning("PDF não encontrado no caminho: {CaminhoArquivo}. Contrato: {Contrato}",
                        caminhoArquivo, adocao.Contrato?.CaminhoArquivoPdf);

                    if (adocao.Contrato?.CaminhoArquivoPdf != null)
                    {
                        var caminhoAlternativo1 = Path.Combine(Directory.GetCurrentDirectory(), adocao.Contrato.CaminhoArquivoPdf);
                        var caminhoAlternativo2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contratos", Path.GetFileName(adocao.Contrato.CaminhoArquivoPdf));

                        _logger.LogInformation("Tentando caminhos alternativos: {Caminho1}, {Caminho2}",
                            caminhoAlternativo1, caminhoAlternativo2);

                        if (System.IO.File.Exists(caminhoAlternativo1))
                        {
                            caminhoArquivo = caminhoAlternativo1;
                        }
                        else if (System.IO.File.Exists(caminhoAlternativo2))
                        {
                            caminhoArquivo = caminhoAlternativo2;
                        }
                    }

                    if (caminhoArquivo == null || !System.IO.File.Exists(caminhoArquivo))
                    {
                        return NotFound($"Arquivo PDF não encontrado. Caminho esperado: {caminhoArquivo}");
                    }
                }

                var bytesArquivo = await System.IO.File.ReadAllBytesAsync(caminhoArquivo);

                Response.Headers.Clear();
                Response.Headers["Content-Type"] = "application/pdf";
                Response.Headers["Content-Disposition"] = "inline; filename=\"contrato.pdf\"";
                Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                Response.Headers["Cache-Control"] = "public, max-age=3600";

                return File(bytesArquivo, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao visualizar contrato {AdocaoId}", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpPost("cancelar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarAdocaoAsync(int id, [FromBody] CancelarAdocaoRequest request)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada." });
                }

                if (adocao.Status.EstaCancelada())
                {
                    return Json(new { sucesso = false, mensagem = "Esta adoção já foi cancelada." });
                }

                if (adocao.Status == StatusAdocao.Finalizado)
                {
                    return Json(new { sucesso = false, mensagem = "Não é possível cancelar uma adoção finalizada." });
                }

                if (string.IsNullOrWhiteSpace(request.Motivo))
                {
                    return Json(new { sucesso = false, mensagem = "É necessário fornecer um motivo para o cancelamento." });
                }

                var statusCancelamento = StatusAdocao.CanceladoPeloCaotinho;

                adocao.Status = statusCancelamento;
                adocao.DataResposta = DateTime.Now;
                adocao.ObservacoesCancelamento = $"Motivo: {request.Motivo}";

                if (!string.IsNullOrWhiteSpace(request.Observacoes))
                {
                    adocao.ObservacoesCancelamento += $"\nObservações: {request.Observacoes}";
                }

                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = StatusPet.Disponivel;
                }

                await _contexto.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.CANCELAR_ADOCAO,
                    $"Adoção cancelada - Pet: {pet?.Nome} (ID: {adocao.PetId})",
                    LogConstants.Categorias.ADOCAO,
                    LogConstants.EntidadesAfetadas.ADOCAO,
                    adocao.Id,
                    LogConstants.NiveisSeveridade.WARNING,
                    detalhesAdicionais: $"Motivo: {request.Motivo}; Observações: {request.Observacoes ?? "Nenhuma"}"
                );

                try
                {
                    var emailEnviado = await _emailService.EnviarEmailCancelamentoAdocaoAsync(adocao.UsuarioId, pet?.Id ?? 0, request.Motivo);
                    if (!emailEnviado)
                    {
                        _logger.LogWarning("Falha ao enviar email de cancelamento para adoção {AdocaoId}", id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar email de cancelamento para adoção {AdocaoId}", id);
                }

                if (adocao.Usuario != null && pet != null)
                {
                    try
                    {
                        await _servicoNotificacao.CriarNotificacaoAsync(
                            adocao.Usuario.Id.ToString(),
                            "Adoção Cancelada",
                            $"Infelizmente, sua adoção do {pet.Nome} foi cancelada. Motivo: {request.Motivo}",
                            "erro",
                            $"adocao_cancelada_{adocao.Id}"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao criar notificação de cancelamento para adoção {AdocaoId}", id);
                    }
                }

                return Json(new { sucesso = true, mensagem = "Adoção cancelada com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar adoção {AdocaoId}", id);
                return Json(new { sucesso = false, mensagem = $"Erro ao cancelar adoção: {ex.Message}" });
            }
        }

        [HttpGet("HistoricoUsuario/{usuarioId}")]
        public async Task<IActionResult> HistoricoUsuarioAsync(int usuarioId)
        {
            try
            {
                var adocoes = await _contexto.Adocoes
                    .Include(a => a.Pet)
                    .Where(a => a.UsuarioId == usuarioId)
                    .OrderByDescending(a => a.DataEnvio)
                    .ToListAsync();

                var historico = adocoes.Select(a => new
                {
                    petNome = a.Pet?.Nome,
                    petEspecie = a.Pet?.Especie.ObterTexto(),
                    dataAdocao = a.DataEnvio,
                    status = a.Status.ObterTexto(),
                    statusCss = a.Status.ToString().ToLower().Replace(" ", "-")
                }).ToList();

                return Json(new { success = true, historico });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar histórico do usuário {UsuarioId}", usuarioId);
                return Json(new { success = false, message = "Erro ao buscar histórico." });
            }
        }

    }
}