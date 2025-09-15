using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models.ViewModels;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using CaotinhoAuMiau.Models.Enums;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Models.ViewModels.Admin;

namespace CaotinhoAuMiau.Controllers.Usuario
{
    [Route("usuario/adocao")]
    [Authorize(Roles = "Usuario")]
    public class AdocaoController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificationService _servicoNotificacao;

        public AdocaoController(ApplicationDbContext contexto, NotificationService servicoNotificacao)
        {
            _contexto = contexto;
            _servicoNotificacao = servicoNotificacao;
        }

        [HttpGet("")]
        [HttpGet("listar")]
        [HttpGet("/usuario/adocoes")]
        public async Task<IActionResult> ListarAsync(
            string? filtroStatus = null,
            string? pesquisa = null,
            string? ordenarPor = "DataEnvio",
            string? direcaoOrdem = "Desc",
            int pagina = 1,
            int itensPorPagina = 10)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToAction("ExibirTelaLogin", "Authentication");
            }

            var viewModel = await PrepararViewModelListagem(
                filtroStatus, pesquisa, ordenarPor, direcaoOrdem, pagina, itensPorPagina, idUsuario);

            return View("~/Views/Usuario/Adocoes.cshtml", viewModel);
        }

        [HttpGet("detalhes/{id}")]
        public async Task<IActionResult> DetalhesAsync(int id)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                TempData["Erro"] = "Usuário não autenticado.";
                return Redirect("/usuario/adocoes");
            }

            try
            {
                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .Include(f => f.Usuario)
                    .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId.ToString() == idUsuario);

                if (formulario == null)
                {
                    return PartialView("~/Views/Usuario/_DetalhesAdocao.cshtml", new { 
                        success = false, 
                        message = "Formulário não encontrado" 
                    });
                }

                if (formulario.Pet == null)
                {
                    return PartialView("~/Views/Usuario/_DetalhesAdocao.cshtml", new { 
                        success = false, 
                        message = "Dados do pet não encontrados" 
                    });
                }

                var adocao = await _contexto.Adocoes
                    .Include(a => a.Contrato)
                    .FirstOrDefaultAsync(a => a.PetId == formulario.PetId &&
                                            a.UsuarioId == formulario.UsuarioId);

                if (adocao == null && formulario.StatusEnum == StatusFormulario.Aprovado)
                {
                    adocao = new Adocao
                    {
                        PetId = formulario.PetId,
                        UsuarioId = formulario.UsuarioId,
                        Status = StatusAdocao.Aprovado,
                        ContratoAssinado = false
                    };

                    _contexto.Adocoes.Add(adocao);
                    await _contexto.SaveChangesAsync();

                    adocao = await _contexto.Adocoes
                        .Include(a => a.Contrato)
                        .FirstOrDefaultAsync(a => a.Id == adocao.Id);
                }

                var resultado = new
                {
                    success = true,
                    pet = new
                    {
                        nome = formulario.Pet?.Nome ?? "Nome não disponível",
                        imagem = formulario.Pet?.NomeArquivoImagem ?? "pet-placeholder.jpg",
                        idade = formulario.Pet?.Idade ?? 0,
                        anos = formulario.Pet?.Anos ?? 0,
                        meses = formulario.Pet?.Meses ?? 0,
                        sexo = formulario.Pet != null ? formulario.Pet.Sexo.ToString() : "Não informado",
                        raca = formulario.Pet?.Raca ?? "SRD",
                        porte = formulario.Pet != null ? formulario.Pet.Porte.ToString() : "Não informado",
                        descricao = formulario.Pet?.Descricao ?? "Sem descrição disponível"
                    },
                    processo = new
                    {
                        id = adocao?.Id ?? formulario.Id,
                        adocaoId = adocao?.Id,
                        formularioId = formulario.Id,
                        status = DeterminarStatusCompleto(formulario, adocao),
                        responsavel = "Equipe CaotinhoAuMiau", 
                        tempoProcesso = CalcularTempoProcesso(formulario.DataEnvio, adocao?.DataFinalizacao ?? DateTime.Now),
                        
                        dataEnvio = formulario.DataEnvio.ToString("dd/MM/yyyy - HH:mm"),
                        dataResposta = formulario.DataResposta?.ToString("dd/MM/yyyy - HH:mm"),
                        dataAprovacao = formulario.StatusEnum == StatusFormulario.Aprovado ? 
                            (formulario.DataResposta?.ToString("dd/MM/yyyy - HH:mm") ?? DateTime.Now.ToString("dd/MM/yyyy - HH:mm")) : null,
                        
                        dataAssinatura = adocao?.Contrato?.DataAssinatura?.ToString("dd/MM/yyyy - HH:mm"),
                        dataFinalizacao = adocao?.DataFinalizacao?.ToString("dd/MM/yyyy - HH:mm"),
                        
                        temContrato = adocao?.Contrato != null || (adocao != null && formulario.StatusEnum == StatusFormulario.Aprovado),
                        contratoAssinado = adocao?.ContratoAssinado == true,
                        temAdocaoCriada = adocao != null,
                        finalizada = adocao?.DataFinalizacao != null,
                        
                        observacaoFormulario = formulario.ObservacaoAdminFormulario,
                        observacoesCancelamento = formulario.ObservacoesCancelamento
                    }
                };

                return PartialView("~/Views/Usuario/_DetalhesAdocao.cshtml", resultado);
            }
            catch (Exception ex)
            {
                
                return PartialView("~/Views/Usuario/_DetalhesAdocao.cshtml", new { 
                    success = false, 
                    message = $"Erro ao carregar detalhes: {ex.Message}" 
                });
            }
        }

        [HttpPost("cancelar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarAsync(int id, string? motivoCancelamento = null)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                TempData["Erro"] = "Usuário não autenticado.";
                return Redirect("/usuario/adocoes");
            }

            try
            {
                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId.ToString() == idUsuario);

                if (formulario == null)
                {
                    TempData["Erro"] = "Formulário não encontrado.";
                    return Redirect("/usuario/adocoes");
                }

                if (formulario.StatusEnum != StatusFormulario.Pendente)
                {
                    TempData["Erro"] = "Só é possível cancelar formulários pendentes.";
                    return Redirect("/usuario/adocoes");
                }

                formulario.StatusEnum = StatusFormulario.CanceladoPeloUsuario;
                if (!string.IsNullOrWhiteSpace(motivoCancelamento))
                {
                    formulario.ObservacoesCancelamento = motivoCancelamento.Trim();
                }

                var outrosFormulariosAprovados = await _contexto.FormulariosAdocao
                    .Where(f => f.PetId == formulario.PetId && f.StatusEnum == StatusFormulario.Aprovado && f.Id != formulario.Id)
                    .AnyAsync();

                if (!outrosFormulariosAprovados && formulario.Pet != null)
                {
                    formulario.Pet.Status = StatusPet.Disponivel;
                    _contexto.Pets.Update(formulario.Pet);
                }

                await _contexto.SaveChangesAsync();

                TempData["Sucesso"] = "Formulário cancelado com sucesso.";
                return Redirect("/usuario/adocoes");
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro interno do servidor.";
                return Redirect("/usuario/adocoes");
            }
        }


        [HttpPost("reativar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReativarAsync(int id)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                TempData["Erro"] = "Usuário não autenticado.";
                return Redirect("/usuario/adocoes");
            }

            try
            {
                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId.ToString() == idUsuario);

                if (formulario == null)
                {
                    TempData["Erro"] = "Formulário não encontrado.";
                    return Redirect("/usuario/adocoes");
                }

                if (formulario.StatusEnum != StatusFormulario.CanceladoPeloUsuario &&
                    formulario.StatusEnum != StatusFormulario.CanceladoPorInatividade)
                {
                    TempData["Erro"] = "Só é possível reativar formulários cancelados.";
                    return Redirect("/usuario/adocoes");
                }

                formulario.StatusEnum = StatusFormulario.Pendente;
                formulario.ObservacoesCancelamento = null;
                formulario.DataEnvio = DateTime.Now;

                await _contexto.SaveChangesAsync();

                TempData["Sucesso"] = "Formulário reativado com sucesso.";
                return Redirect("/usuario/adocoes");
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro interno do servidor.";
                return Redirect("/usuario/adocoes");
            }
        }

        private async Task<AdocaoListaViewModel> PrepararViewModelListagem(
            string? filtroStatus, string? pesquisa, string? ordenarPor, string? direcaoOrdem,
            int pagina, int itensPorPagina, string idUsuario)
        {
            var usuarioIdInt = int.Parse(idUsuario);
            var query = _contexto.FormulariosAdocao
                .Include(f => f.Pet)
                .Include(f => f.Usuario)
                .Where(f => f.UsuarioId.ToString() == idUsuario);

            if (!string.IsNullOrEmpty(filtroStatus) && filtroStatus != "all")
            {
                switch (filtroStatus)
                {
                    case "Pendente":
                        query = query.Where(f => f.StatusEnum == StatusFormulario.Pendente || f.StatusEnum == StatusFormulario.EmAnalise);
                        break;
                    case "Aprovado":
                        query = query.Where(f => f.StatusEnum == StatusFormulario.Aprovado);
                        break;
                    case "Finalizada":
                        var adocoesFinalizadas = _contexto.Adocoes
                            .Where(a => a.UsuarioId == usuarioIdInt && a.Status == StatusAdocao.Finalizado)
                            .Select(a => a.PetId)
                            .ToList();
                        query = query.Where(f => f.StatusEnum == StatusFormulario.Aprovado && adocoesFinalizadas.Contains(f.PetId));
                        break;
                }
            }

            if (!string.IsNullOrEmpty(pesquisa))
            {
                query = query.Where(f => f.Pet!.Nome.Contains(pesquisa) || 
                                      f.Pet.Raca.Contains(pesquisa));
            }

            query = ordenarPor?.ToLower() switch
            {
                "recent" => query.OrderByDescending(f => f.DataEnvio),
                "oldest" => query.OrderBy(f => f.DataEnvio),
                "name" => query.OrderBy(f => f.Pet!.Nome),
                "status" => query.OrderBy(f => f.StatusEnum),
                _ => query.OrderByDescending(f => f.DataEnvio)
            };

            var totalItens = await query.CountAsync();

            var formularios = await query
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            var petIds = formularios.Select(f => f.PetId).ToList();
            var adocoes = await _contexto.Adocoes
                .Include(a => a.Contrato)
                .Where(a => petIds.Contains(a.PetId) && a.UsuarioId == usuarioIdInt)
                .ToListAsync();

            var adocoesViewModel = formularios.Select(f =>
            {
                var adocao = adocoes.FirstOrDefault(a => a.PetId == f.PetId);
                
                return new AdocaoUsuarioSummaryViewModel
                {
                    Id = f.Id,
                    AdocaoId = adocao?.Id,
                    StatusAdocao = adocao?.Status,
                    StatusFormulario = f.StatusEnum,
                    DataEnvio = f.DataEnvio,
                    DataResposta = f.DataResposta,
                    DataAssinatura = adocao?.Contrato?.DataAssinatura,
                    DataFinalizacao = adocao?.DataFinalizacao,
                    DataCriacaoContrato = adocao?.Contrato?.DataCriacao,
                    PetId = f.PetId,
                    PetNome = f.Pet?.Nome ?? "",
                    PetImagem = f.Pet?.NomeArquivoImagem,
                    PetEspecie = f.Pet?.Especie ?? Especie.Cao,
                    PetRaca = f.Pet?.Raca ?? "SRD",
                    PetIdade = f.Pet?.Idade ?? 0,
                    PetAnos = f.Pet?.Anos ?? 0,
                    PetMeses = f.Pet?.Meses ?? 0,
                    PetSexo = f.Pet?.Sexo ?? SexoPet.Macho,
                    PetDescricao = f.Pet?.Descricao
                };
            }).ToList();

            var baseStatsQuery = _contexto.FormulariosAdocao
                .Where(f => f.UsuarioId == usuarioIdInt);
            
            var allQuery = _contexto.FormulariosAdocao.Where(f => f.UsuarioId == usuarioIdInt);
            var allFormularios = await allQuery.ToListAsync();
            var allPetIds = allFormularios.Select(f => f.PetId).ToList();
            var todasAdocoes = await _contexto.Adocoes
                .Where(a => a.UsuarioId == usuarioIdInt && allPetIds.Contains(a.PetId))
                .ToListAsync();

            var statistics = new AdocaoStatisticsUsuarioViewModel
            {
                TotalSolicitacoes = allFormularios.Count,
                EmAnalise = allFormularios.Count(f => f.StatusEnum == StatusFormulario.Pendente || f.StatusEnum == StatusFormulario.EmAnalise),
                Aprovadas = allFormularios.Count(f => f.StatusEnum == StatusFormulario.Aprovado),
                Concluidas = allFormularios.Count(f => f.StatusEnum == StatusFormulario.Aprovado &&
                    todasAdocoes.Any(a => a.PetId == f.PetId && a.Status == StatusAdocao.Finalizado))
            };

            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id.ToString() == idUsuario);

            return new AdocaoListaViewModel
            {
                Adocoes = adocoesViewModel,
                Statistics = statistics,
                FilterOptions = AdocaoFilterOptionsUsuarioViewModel.Create(),
                Pagination = new PaginationViewModel
                {
                    PaginaAtual = pagina,
                    TotalItens = totalItens,
                    ItensPorPagina = itensPorPagina,
                    TotalPaginas = (int)Math.Ceiling((double)totalItens / itensPorPagina)
                },
                UsuarioNome = usuario?.Nome ?? User.Identity?.Name ?? "Usuário",
                FotoPerfilUsuario = usuario?.FotoPerfil,
                FiltroStatus = filtroStatus,
                Pesquisa = pesquisa,
                OrdenarPor = ordenarPor,
                DirecaoOrdem = direcaoOrdem
            };
        }

        private string DeterminarStatusCompleto(FormularioAdocao formulario, Adocao? adocao)
        {
            try
            {
                if (adocao == null && formulario != null)
                {
                    return formulario.StatusEnum.ObterTexto();
                }
                
                if (adocao != null)
                {
                    return adocao.Status.ObterTexto();
                }
                
                return "Pendente";
            }
            catch (Exception)
            {
                return "Pendente"; // Status padrão em caso de erro
            }
        }

        private string CalcularTempoProcesso(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var diferenca = dataFim - dataInicio;
                
                if (diferenca.Days > 0)
                    return $"{diferenca.Days} dia{(diferenca.Days > 1 ? "s" : "")}";
                else if (diferenca.Hours > 0)
                    return $"{diferenca.Hours} hora{(diferenca.Hours > 1 ? "s" : "")}";
                else
                    return "Menos de 1 hora";
            }
            catch (Exception)
            {
                return "Tempo não calculado";
            }
        }

        [HttpGet("formulario/{petId}")]
        public IActionResult RedirecionarFormulario(int petId)
        {
            return Redirect($"/usuario/formulario-adocao/{petId}");
        }

        [HttpPost("formulario/{petId}")]
        public IActionResult RedirecionarFormularioPost(int petId)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head><title>Redirecionando...</title></head>
<body>
<form id='redirectForm' action='/usuario/formulario-adocao/{petId}' method='post'>
";
            foreach (var key in Request.Form.Keys)
            {
                if (!key.StartsWith("__"))
                {
                    html += $"<input type='hidden' name='{key}' value='{Request.Form[key]}' />";
                }
            }
            html += @"
</form>
<script>document.getElementById('redirectForm').submit();</script>
</body>
</html>";
            return Content(html, "text/html");
        }

    }
}