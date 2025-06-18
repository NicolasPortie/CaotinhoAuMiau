using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using CaotinhoAuMiau.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Authorize(Roles = "Administrador")]
    [Route("admin/formularios-adocao")]
    public class GerenciamentoFormularioAdocaoController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificacaoServico _servicoNotificacao;

        public GerenciamentoFormularioAdocaoController(ApplicationDbContext contexto, NotificacaoServico servicoNotificacao)
        {
            _contexto = contexto;
            _servicoNotificacao = servicoNotificacao;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(int pagina = 1, int itensPorPagina = 10, string filtroStatus = "", string filtroData = "", string pesquisa = "")
        {
            var adminId = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login", "Autenticacao");
            }
            
            var query = _contexto.FormulariosAdocao
                .Include(f => f.Pet)
                .Include(f => f.Usuario)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(filtroStatus))
            {
                query = query.Where(f => f.Status.ToLower() == filtroStatus.ToLower());
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
        public async Task<IActionResult> ObterDetalhesFormulario(int id)
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
                    status = formulario.Status,
                    observacoesAdmin = formulario.ObservacaoAdminFormulario
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
        public async Task<IActionResult> AprovarFormulario(int id, string observacao = "")
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

                if (pet.Status == "Em Processo")
                {
                    var formularioAprovado = await _contexto.FormulariosAdocao
                        .FirstOrDefaultAsync(f => f.PetId == pet.Id && f.Status == "Aprovado" && f.Id != id);
                        
                    if (formularioAprovado != null)
                    {
                        return Json(new { sucesso = false, mensagem = $"Este pet já está em processo de adoção por outro usuário. Status atual: {pet.Status}" });
                    }
                    
                }
                else if (pet.Status != "Disponível" && pet.Status != "")
                {
                    return Json(new { sucesso = false, mensagem = $"Este pet não está disponível para adoção. Status atual: {pet.Status}" });
                }

                var outrosFormularios = await _contexto.FormulariosAdocao
                    .Where(f => f.PetId == pet.Id && f.Id != id && f.Status == "Pendente")
                    .ToListAsync();
                    
                foreach (var outroForm in outrosFormularios)
                {
                    outroForm.Status = "Rejeitada";
                    outroForm.DataResposta = DateTime.Now;
                    outroForm.ObservacaoAdminFormulario = "Formulário rejeitado automaticamente porque outro candidato foi aprovado para adotar este pet.";
                    
                    try
                    {
                        await _servicoNotificacao.CriarNotificacao(
                            outroForm.UsuarioId.ToString(),
                            "Formulário de adoção rejeitado",
                            $"Seu formulário de adoção para o pet {pet.Nome} foi rejeitado porque outro candidato foi aprovado.",
                            "FormularioAdocao",
                            outroForm.Id.ToString()
                        );
                    }
                    catch (Exception notifEx)
                    {
                        Console.WriteLine($"Erro ao enviar notificação: {notifEx.Message}");
                    }
                }

                formulario.Status = "Aprovado";
                formulario.DataResposta = DateTime.Now;
                
                if (!string.IsNullOrWhiteSpace(observacao))
                {
                    formulario.ObservacaoAdminFormulario = observacao;
                }

                pet.Status = "Em Processo";

                var adocao = new Adocao
                {
                    PetId = formulario.PetId,
                    UsuarioId = formulario.UsuarioId,
                    DataEnvio = formulario.DataEnvio,
                    DataResposta = DateTime.Now,
                    Status = "Aguardando buscar"
                };

                _contexto.Adocoes.Add(adocao);

                if (formulario.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacao(
                        formulario.UsuarioId.ToString(),
                        "Formulário de adoção aprovado",
                        $"Seu formulário de adoção para o pet {pet.Nome} foi aprovado! O pet está aguardando retirada.",
                        "Adocao",
                        formulario.Id.ToString()
                    );
                }

                await _contexto.SaveChangesAsync();

                return Json(new { sucesso = true, mensagem = "Formulário aprovado com sucesso! Uma nova adoção foi criada com status 'Aguardando buscar'." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao aprovar formulário: {ex.Message}" });
            }
        }

        [HttpPost("Rejeitar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarFormulario(int id, string motivo)
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

                formulario.Status = "Rejeitada";
                formulario.DataResposta = DateTime.Now;
                formulario.ObservacaoAdminFormulario = motivo;

                if (formulario.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacao(
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
        public async Task<IActionResult> SolicitarInformacoesAdicionais(int id, [FromBody] string observacao)
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

                formulario.Status = "Informações Pendentes";
                formulario.ObservacaoAdminFormulario = observacao;
                formulario.DataResposta = DateTime.Now;

                if (formulario.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacao(
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
