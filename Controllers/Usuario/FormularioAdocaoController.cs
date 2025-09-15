using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models.ViewModels;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Models.Enums;

namespace CaotinhoAuMiau.Controllers.Usuario
{
    [Route("usuario/formulario-adocao")]
    [Authorize(Roles = "Usuario")]
    public class FormularioAdocaoController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificationService _servicoNotificacao;
        public FormularioAdocaoController(ApplicationDbContext contexto, NotificationService servicoNotificacao)
        {
            _contexto = contexto;
            _servicoNotificacao = servicoNotificacao;
        }

        [HttpGet("{petId}")]
        public async Task<IActionResult> ExibirFormularioAsync(int petId)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToAction("ExibirTelaLogin", "Authentication");
            }

            var pet = await _contexto.Pets
                .FirstOrDefaultAsync(p => p.Id == petId);

            if (pet == null || pet.Status != Models.Enums.StatusPet.Disponivel)
            {
                TempData["Erro"] = "Pet não encontrado ou não está mais disponível.";
                return RedirectToAction("ExplorarPets", "Pet");
            }

            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Id.ToString() == idUsuario);
            if (usuario == null)
            {
                return RedirectToAction("ExibirTelaLogin", "Authentication");
            }

            var viewModel = FormularioAdocaoViewModel.Criar(pet, usuario);

            return View("~/Views/Usuario/FormularioAdocao.cshtml", viewModel);
        }

        [HttpPost("{petId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessarFormularioAsync(int petId, FormularioAdocaoViewModel viewModel)
        {
            try
            {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToAction("ExibirTelaLogin", "Authentication");
            }

            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => x.Value.Errors.First().ErrorMessage)
                        .ToList();
                    return Json(new { success = false, errors = errors });
                }
                TempData["Erro"] = "Por favor, preencha todos os campos obrigatórios.";
                return View("~/Views/Usuario/FormularioAdocao.cshtml", viewModel);
            }

            var pet = await _contexto.Pets.FirstOrDefaultAsync(p => p.Id == petId);
            if (pet == null || pet.Status != Models.Enums.StatusPet.Disponivel)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Pet não encontrado ou não está mais disponível." });
                }
                TempData["Erro"] = "Pet não encontrado ou não está mais disponível.";
                return RedirectToAction("ExplorarPets", "Pet");
            }

            var formularioExistente = await _contexto.FormulariosAdocao
                .FirstOrDefaultAsync(f => f.PetId == petId && f.UsuarioId == int.Parse(idUsuario) && f.StatusEnum == StatusFormulario.Pendente);

            if (formularioExistente != null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Você já possui um formulário de adoção pendente para este pet." });
                }
                TempData["Erro"] = "Você já possui um formulário de adoção pendente para este pet.";
                return RedirectToAction("ExplorarPets", "Pet");
            }


            var formulario = new FormularioAdocao
            {
                PetId = petId,
                UsuarioId = int.Parse(idUsuario),
                EspacoAdequado = viewModel.EspacoAdequado ?? string.Empty,
                ExperienciaAnterior = viewModel.ExperienciaAnterior ?? string.Empty,
                MotivacaoAdocao = viewModel.MotivacaoAdocao ?? string.Empty,
                CondicoesFinanceiras = viewModel.CondicoesFinanceiras ?? string.Empty,
                PlanejamentoViagens = viewModel.PlanejamentoViagens ?? string.Empty,
                RendaMensal = viewModel.RendaMensal,
                NumeroMoradores = viewModel.NumeroMoradores,
                DescricaoMoradia = viewModel.DescricaoMoradia ?? string.Empty,
                TempoDisponivel = viewModel.TempoDisponivel,
                TevePetAnterior = viewModel.TevePet,
                StatusEnum = StatusFormulario.Pendente,
                DataEnvio = DateTime.Now
            };

            _contexto.FormulariosAdocao.Add(formulario);

            pet.Status = StatusPet.EmProcesso;
            _contexto.Pets.Update(pet);

            await _contexto.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new {
                    success = true,
                    message = "Formulário enviado com sucesso! Nossa equipe analisará sua solicitação em breve.",
                    redirectUrl = "/usuario/adocao"
                });
            }

            TempData["Sucesso"] = "Formulário enviado com sucesso! Nossa equipe analisará sua solicitação em breve.";
            return Redirect("/usuario/adocao");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO FORMULÁRIO] Exceção: {ex.Message}");
                Console.WriteLine($"[ERRO FORMULÁRIO] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ERRO FORMULÁRIO] Inner exception: {ex.InnerException.Message}");
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Erro ao enviar formulário: {ex.Message}" });
                }
                TempData["Erro"] = "Erro ao enviar formulário. Tente novamente.";
                return View("~/Views/Usuario/FormularioAdocao.cshtml", viewModel);
            }
        }

        [HttpPost("cancelar/{id}")]
        public async Task<IActionResult> CancelarFormularioAdocaoAsync(int id, [FromBody] CancelamentoRequest request)
        {
            try
            {
                var idUsuario = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(idUsuario))
                {
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId.ToString() == idUsuario);

                if (formulario == null)
                {
                    return Json(new { success = false, message = "Formulário não encontrado" });
                }

                if (formulario.StatusEnum != StatusFormulario.Pendente)
                {
                    return Json(new { success = false, message = "Só é possível cancelar formulários pendentes" });
                }

                formulario.StatusEnum = StatusFormulario.CanceladoPeloUsuario;
                if (!string.IsNullOrWhiteSpace(request?.MotivoCancelamento))
                {
                    formulario.ObservacoesCancelamento = request.MotivoCancelamento.Trim();
                }

                await _contexto.SaveChangesAsync();

                return Json(new { success = true, message = "Formulário cancelado com sucesso" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpPost("reativar/{id}")]
        public async Task<IActionResult> ReativarFormularioAdocaoAsync(int id)
        {
            try
            {
                var idUsuario = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(idUsuario))
                {
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId.ToString() == idUsuario);

                if (formulario == null)
                {
                    return Json(new { success = false, message = "Formulário não encontrado" });
                }

                if (formulario.StatusEnum != StatusFormulario.CanceladoPeloUsuario)
                {
                    return Json(new { success = false, message = "Só é possível reativar formulários cancelados" });
                }

                formulario.StatusEnum = StatusFormulario.Pendente;
                formulario.ObservacoesCancelamento = null;
                formulario.DataEnvio = DateTime.Now;

                await _contexto.SaveChangesAsync();

                return Json(new { success = true, message = "Formulário reativado com sucesso" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpGet("detalhes/{formularioId}")]
        public async Task<IActionResult> ObterDetalhesFormularioAsync(int formularioId)
        {
            return await ObterDetalhesInternoAsync(formularioId, true);
        }

        [HttpGet("detalhes-por-adocao/{adocaoId}")]
        public async Task<IActionResult> ObterDetalhesPorAdocaoAsync(int adocaoId)
        {
            return await ObterDetalhesInternoAsync(adocaoId, false);
        }

        private async Task<IActionResult> ObterDetalhesInternoAsync(int id, bool isFormularioId)
        {
            try
            {
                var idUsuario = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(idUsuario))
                {
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }

                FormularioAdocao formulario;
                
                if (isFormularioId)
                {
                    formulario = await _contexto.FormulariosAdocao
                        .Include(f => f.Pet)
                        .Include(f => f.Usuario)
                        .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId.ToString() == idUsuario);
                }
                else
                {
                    var adocaoTemp = await _contexto.Adocoes
                        .FirstOrDefaultAsync(a => a.Id == id && a.UsuarioId.ToString() == idUsuario);
                        
                    if (adocaoTemp == null)
                    {
                        return Json(new { success = false, message = "Adoção não encontrada" });
                    }
                    
                    formulario = await _contexto.FormulariosAdocao
                        .Include(f => f.Pet)
                        .Include(f => f.Usuario)
                        .FirstOrDefaultAsync(f => f.PetId == adocaoTemp.PetId && f.UsuarioId == adocaoTemp.UsuarioId);
                }

                if (formulario == null)
                {
                    return Json(new { success = false, message = "Formulário não encontrado" });
                }

                var adocao = await _contexto.Adocoes
                    .Include(a => a.Contrato)
                    .FirstOrDefaultAsync(a => a.PetId == formulario.PetId && 
                                            a.UsuarioId == formulario.UsuarioId);

                var resultado = new
                {
                    success = true,
                    pet = new
                    {
                        nome = formulario.Pet?.Nome,
                        imagem = formulario.Pet?.NomeArquivoImagem,
                        idade = formulario.Pet?.Idade,
                        sexo = formulario.Pet?.Sexo.ToString(),
                        raca = formulario.Pet?.Raca,
                        porte = formulario.Pet?.Porte.ToString(),
                        descricao = formulario.Pet?.Descricao
                    },
                    processo = new
                    {
                        id = formulario.Id,
                        status = DeterminarStatusCompleto(formulario, adocao),
                        responsavel = "Equipe CaotinhoAuMiau", 
                        tempoProcesso = CalcularTempoProcesso(formulario.DataEnvio, adocao?.DataFinalizacao ?? DateTime.Now),
                        
                        dataEnvio = formulario.DataEnvio.ToString("dd/MM/yyyy - HH:mm"),
                        dataResposta = formulario.DataResposta?.ToString("dd/MM/yyyy - HH:mm"),
                        dataAprovacao = formulario.StatusEnum == StatusFormulario.Aprovado ?
                            (formulario.DataResposta?.ToString("dd/MM/yyyy - HH:mm") ?? DateTime.Now.ToString("dd/MM/yyyy - HH:mm")) : null,

                        dataAssinatura = adocao?.Contrato?.DataAssinatura?.ToString("dd/MM/yyyy - HH:mm"),
                        dataFinalizacao = adocao?.DataFinalizacao?.ToString("dd/MM/yyyy - HH:mm"),
                        
                        temContrato = adocao?.Contrato != null,
                        contratoAssinado = adocao?.ContratoAssinado == true,
                        finalizada = adocao?.DataFinalizacao != null,

                        observacaoFormulario = formulario.ObservacaoAdminFormulario,
                        observacoesCancelamento = formulario.ObservacoesCancelamento
                    }
                };

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro interno do servidor", error = ex.Message });
            }
        }

        private string DeterminarStatusCompleto(FormularioAdocao formulario, Adocao? adocao)
        {
            if (adocao == null)
            {
                return formulario.Status;
            }

            if (adocao.DataFinalizacao != null)
            {
                return "Finalizada";
            }

            if (adocao.ContratoAssinado == true)
            {
                return "Contrato Assinado";
            }

            if (adocao.Contrato != null)
            {
                return "Contrato Pendente";
            }

            return adocao.Status.ToString() ?? formulario.Status;
        }

        private string CalcularTempoProcesso(DateTime dataInicio, DateTime dataFim)
        {
            var diferenca = dataFim - dataInicio;
            
            if (diferenca.Days > 0)
                return $"{diferenca.Days} dia{(diferenca.Days > 1 ? "s" : "")}";
            else if (diferenca.Hours > 0)
                return $"{diferenca.Hours} hora{(diferenca.Hours > 1 ? "s" : "")}";
            else
                return "Menos de 1 hora";
        }
    }

    public class CancelamentoRequest
    {
        public string MotivoCancelamento { get; set; } = string.Empty;
    }
}