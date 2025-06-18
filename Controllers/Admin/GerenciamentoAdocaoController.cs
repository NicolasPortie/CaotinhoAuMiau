using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Services;
using System.Text.Json;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Authorize(Roles = "Administrador")]
    [Route("admin/adocoes")]
    public class GerenciamentoAdocaoController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificacaoServico _servicoNotificacao;

        public GerenciamentoAdocaoController(ApplicationDbContext contexto, NotificacaoServico servicoNotificacao)
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
            
            var query = _contexto.Adocoes
                .Include(a => a.Pet)
                .Include(a => a.Usuario)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(filtroStatus))
            {
                query = query.Where(a => a.Status.ToLower() == filtroStatus.ToLower());
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
                    (a.Usuario.Nome != null && a.Usuario.Nome.ToLower().Contains(pesquisa)) ||
                    (a.Usuario.Email != null && a.Usuario.Email.ToLower().Contains(pesquisa)) ||
                    (a.Pet.Nome != null && a.Pet.Nome.ToLower().Contains(pesquisa)) ||
                    (a.Status != null && a.Status.ToLower().Contains(pesquisa))
                );
            }
            
            var totalItens = await query.CountAsync();
            
            var adocoesPaginadas = await query
                .OrderByDescending(a => a.DataEnvio)
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

            return View("~/Views/Admin/GerenciamentoAdocoes.cshtml", adocoesPaginadas);
        }

        [HttpGet("Detalhes/{id}")]
        public async Task<IActionResult> ObterDetalhesAdocao(int id)
        {
            var formulario = await _contexto.FormulariosAdocao
                .Include(a => a.Pet)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (formulario == null)
            {
                return Json(new { sucesso = false, mensagem = "Adoção não encontrada." });
            }
            
            var result = new
            {
                id = formulario.Id,
                status = formulario.Status,
                dataInicio = formulario.DataEnvio,
                dataResposta = formulario.DataResposta,
                observacaoAdminFormulario = formulario.ObservacaoAdminFormulario,
                observacoesCancelamento = formulario.ObservacoesCancelamento,
                pet = new
                {
                    id = formulario.Pet.Id,
                    nome = formulario.Pet.Nome,
                    especie = formulario.Pet.Especie,
                    raca = formulario.Pet.Raca,
                    porte = formulario.Pet.Porte,
                    sexo = formulario.Pet.Sexo,
                    anos = formulario.Pet.Anos,
                    meses = formulario.Pet.Meses,
                    imagem = formulario.Pet.NomeArquivoImagem
                },
                adotante = new
                {
                    id = formulario.Usuario.Id,
                    nome = formulario.Usuario.Nome,
                    email = formulario.Usuario.Email,
                    telefone = formulario.Usuario.Telefone,
                    cpf = formulario.Usuario.CPF
                }
            };

            return Json(new { sucesso = true, dados = result });
        }

        [HttpPost("aprovar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarAdocao(int id)
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
                
                if (adocao.Status == "Aprovado" || adocao.Status == "Rejeitada" ||
                    adocao.Status == "Cancelada")
                {
                    return Json(new { sucesso = false, mensagem = $"Esta adoção já foi {adocao.Status.ToLower()}." });
                }
                
                adocao.Status = "Aprovado";
                adocao.DataResposta = DateTime.Now;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = "Em Processo";
                }
                
                if (adocao.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacao(
                        adocao.UsuarioId.ToString(),
                        "Solicitação de adoção aprovada",
                        $"Sua solicitação de adoção para o pet {pet?.Nome} foi aprovada!",
                        "Adocao",
                        adocao.Id.ToString()
                    );
                }
                
                await _contexto.SaveChangesAsync();
                
                return Json(new { sucesso = true, mensagem = "Adoção aprovada com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao aprovar adoção: {ex.Message}" });
            }
        }

        [HttpPost("rejeitar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarAdocao(int id, string motivo)
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
                
                if (adocao.Status == "Aprovado" || adocao.Status == "Rejeitada" ||
                    adocao.Status == "Cancelada")
                {
                    return Json(new { sucesso = false, mensagem = $"Esta adoção já foi {adocao.Status.ToLower()}." });
                }
                
                adocao.Status = "Rejeitada";
                adocao.DataResposta = DateTime.Now;
                adocao.ObservacoesCancelamento = motivo;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = "Disponível";
                }
                
                if (adocao.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacao(
                        adocao.UsuarioId.ToString(),
                        "Solicitação de adoção rejeitada",
                        $"Sua solicitação de adoção para o pet {pet?.Nome} foi rejeitada. Motivo: {motivo}",
                        "Adocao",
                        adocao.Id.ToString()
                    );
                }
                
                await _contexto.SaveChangesAsync();
                
                return Json(new { sucesso = true, mensagem = "Adoção rejeitada com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao rejeitar adoção: {ex.Message}" });
            }
        }

        [HttpPost("finalizar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarAdocao(int id)
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
                
                if (adocao.Status != "Aguardando buscar")
                {
                    return Json(new { 
                        sucesso = false, 
                        mensagem = $"Apenas adoções com status 'Aguardando buscar' podem ser finalizadas. Status atual: '{adocao.Status}'" 
                    });
                }
                
                adocao.Status = "Finalizada";
                adocao.DataFinalizacao = DateTime.Now;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = "Adotado";
                }
                
                var formulario = await _contexto.FormulariosAdocao
                    .FirstOrDefaultAsync(f => f.PetId == adocao.PetId && f.UsuarioId == adocao.UsuarioId);

                if (formulario != null)
                {
                    if (formulario.Status != "Finalizada")
                    {
                        formulario.Status = "Finalizada";
                    }
                    if (formulario.DataResposta == null)
                    {
                        formulario.DataResposta = DateTime.Now;
                    }
                }
                
                if (adocao.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacao(
                        adocao.UsuarioId.ToString(),
                        "Adoção finalizada",
                        $"A adoção do pet {adocao.Pet?.Nome} foi finalizada com sucesso!",
                        "Adocao",
                        adocao.Id.ToString()
                    );
                }
                
                await _contexto.SaveChangesAsync();
                
                return Json(new { 
                    sucesso = true, 
                    mensagem = "Adoção finalizada com sucesso!" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao finalizar adoção: {ex.Message}" });
            }
        }

        [HttpPost("CancelarAdocao/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarAdocao(int id, string motivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motivo))
                {
                    TempData["Erro"] = "É necessário fornecer um motivo para o cancelamento.";
                    return RedirectToAction(nameof(Listar));
                }
                
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == id);
                    
                if (adocao == null)
                {
                    TempData["Erro"] = "Adoção não encontrada.";
                    return RedirectToAction(nameof(Listar));
                }
                
                if (adocao.Status == "Finalizada" || adocao.Status == "Cancelada")
                {
                    TempData["Erro"] = $"Não é possível cancelar uma adoção que já está {adocao.Status.ToLower()}.";
                    return RedirectToAction(nameof(Listar));
                }
                
                var observacaoCancelamento = $"Cancelado pelo Administrador: {motivo}";
                
                adocao.Status = "Cancelado pelo Admin";
                adocao.DataFinalizacao = DateTime.Now;
                adocao.ObservacoesCancelamento = observacaoCancelamento;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = "Disponível";
                }
                
                var formulario = await _contexto.FormulariosAdocao
                    .FirstOrDefaultAsync(f => f.PetId == adocao.PetId && f.UsuarioId == adocao.UsuarioId);
                    
                if (formulario != null)
                {
                    formulario.ObservacoesCancelamento = observacaoCancelamento;
                }
                
                try
                {
                    if (adocao.Usuario != null)
                    {
                        await _servicoNotificacao.CriarNotificacao(
                            adocao.UsuarioId.ToString(),
                            "Adoção cancelada",
                            $"A adoção do pet {adocao.Pet?.Nome} foi cancelada pelo administrador. Motivo: {motivo}",
                            "Adocao",
                            adocao.Id.ToString()
                        );
                    }
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"Erro ao criar notificação: {notifEx.Message}");
                }
                
                await _contexto.SaveChangesAsync();
                
                TempData["Sucesso"] = "Adoção cancelada com sucesso!";
                return RedirectToAction(nameof(Listar));
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao cancelar adoção: {ex.Message}";
                return RedirectToAction(nameof(Listar));
            }
        }

        [HttpPost("aguardando-buscar/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarAguardandoRetirada(int id)
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
                
                if (adocao.Status != "Aprovado")
                {
                    return Json(new { 
                        sucesso = false, 
                        mensagem = $"Apenas adoções com status 'Aprovado' podem ser marcadas para retirada. Status atual: '{adocao.Status}'" 
                    });
                }
                
                adocao.Status = "Aguardando buscar";
                adocao.DataResposta = DateTime.Now;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null && pet.Status != "Em Processo")
                {
                    pet.Status = "Em Processo";
                }
                
                if (adocao.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacao(
                        adocao.UsuarioId.ToString(),
                        "Pet pronto para retirada",
                        $"O pet {adocao.Pet?.Nome} está pronto para ser retirado. Entre em contato para agendar a retirada.",
                        "Adocao",
                        adocao.Id.ToString()
                    );
                }
                
                await _contexto.SaveChangesAsync();
                
                return Json(new { 
                    sucesso = true, 
                    mensagem = "Pet marcado como pronto para retirada!" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao marcar pet como pronto para retirada: {ex.Message}" });
            }
        }

        [HttpPost("marcarParaRetirada/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarParaRetirada(int id)
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
                
                if (adocao.Status != "Aprovado")
                {
                    return Json(new { 
                        sucesso = false, 
                        mensagem = $"Apenas adoções com status 'Aprovado' podem ser marcadas para retirada. Status atual: '{adocao.Status}'" 
                    });
                }
                
                adocao.Status = "Aguardando buscar";
                adocao.DataResposta = DateTime.Now;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null && pet.Status != "Em Processo")
                {
                    pet.Status = "Em Processo";
                }

                var formulario = await _contexto.FormulariosAdocao
                    .FirstOrDefaultAsync(f => f.PetId == adocao.PetId && f.UsuarioId == adocao.UsuarioId);

                if (formulario != null)
                {
                    formulario.Status = "Aguardando buscar";
                    if (formulario.DataResposta == null)
                    {
                        formulario.DataResposta = DateTime.Now;
                    }
                }
                
                if (adocao.Usuario != null)
                {
                    await _servicoNotificacao.CriarNotificacao(
                        adocao.UsuarioId.ToString(),
                        "Pet pronto para retirada",
                        $"O pet {adocao.Pet?.Nome} está pronto para ser retirado. Entre em contato para agendar a retirada.",
                        "Adocao",
                        adocao.Id.ToString()
                    );
                }
                
                await _contexto.SaveChangesAsync();
                
                return Json(new { 
                    sucesso = true, 
                    mensagem = "Pet marcado para retirada com sucesso!" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao marcar pet para retirada: {ex.Message}" });
            }
        }

        [HttpPost("MarcarRetirado/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarRetirado(int id)
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

                if (adocao.Status == "Finalizada")
                {
                    return Json(new { sucesso = false, mensagem = "Esta adoção já está finalizada." });
                }

                if (adocao.Status == "Cancelada")
                {
                    return Json(new { sucesso = false, mensagem = "Não é possível finalizar uma adoção cancelada." });
                }

                if (adocao.Status != "Aguardando buscar" && adocao.Status != "Aprovado")
                {
                    return Json(new { sucesso = false, mensagem = $"Não é possível finalizar uma adoção com status '{adocao.Status}'. A adoção deve estar em 'Aguardando buscar' ou 'Aprovado'." });
                }

                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet == null)
                {
                    return Json(new { sucesso = false, mensagem = "Pet não encontrado." });
                }

                pet.Status = "Adotado";

                adocao.Status = "Finalizada";
                adocao.DataFinalizacao = DateTime.Now;
                
                var formulario = await _contexto.FormulariosAdocao
                    .FirstOrDefaultAsync(f => f.PetId == adocao.PetId && f.UsuarioId == adocao.UsuarioId);

                if (formulario != null)
                {
                    if (formulario.DataResposta == null)
                    {
                        formulario.DataResposta = DateTime.Now;
                    }
                }

                try
                {
                    await _servicoNotificacao.CriarNotificacao(
                        adocao.UsuarioId.ToString(),
                        "Adoção finalizada com sucesso",
                        $"A adoção do pet {pet.Nome} foi finalizada com sucesso! Obrigado por adotar um amigo.",
                        "Adocao",
                        adocao.Id.ToString()
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao criar notificação: {ex.Message}");
                }

                await _contexto.SaveChangesAsync();

                return Json(new { sucesso = true, mensagem = "Adoção finalizada com sucesso! O status do formulário permanece como Aprovado." });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao marcar adoção como finalizada: {ex.Message}" });
            }
        }

        [HttpGet("DetalhesAdocao/{id}")]
        public async Task<IActionResult> DetalhesAdocao(int id)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Pet)
                    .Include(a => a.Usuario)
                    .FirstOrDefaultAsync(a => a.Id == id);
                    
                if (adocao == null)
                {
                    var formulario = await _contexto.FormulariosAdocao
                        .Include(f => f.Pet)
                        .Include(f => f.Usuario)
                        .FirstOrDefaultAsync(f => f.Id == id);
                        
                    if (formulario == null)
                    {
                        return NotFound(new { sucesso = false, mensagem = "Adoção não encontrada" });
                    }
                    
                    if (formulario.Pet == null || formulario.Usuario == null)
                    {
                        return NotFound(new { sucesso = false, mensagem = "Dados incompletos do formulário" });
                    }
                    
                    return Json(new {
                        id = formulario.Id,
                        dataEnvio = formulario.DataEnvio,
                        dataPedido = formulario.DataEnvio,
                        dataResposta = formulario.DataResposta,
                        dataFinalizacao = (DateTime?)null,
                        dataCancelamento = (DateTime?)null,
                        status = formulario.Status ?? "Pendente",
                        motivoCancelamento = formulario.ObservacoesCancelamento,
                        observacoesCancelamento = formulario.ObservacoesCancelamento,
                        observacaoAdminFormulario = formulario.ObservacaoAdminFormulario,
                        
                        pet = new {
                            id = formulario.Pet.Id,
                            nome = formulario.Pet.Nome ?? "Não informado",
                            especie = formulario.Pet.Especie ?? "Não informada",
                            raca = formulario.Pet.Raca ?? "Não informada",
                            idade = (formulario.Pet.Anos > 0 ? $"{formulario.Pet.Anos} anos" : "") + 
                                   (formulario.Pet.Meses > 0 ? $" e {formulario.Pet.Meses} meses" : ""),
                            anos = formulario.Pet.Anos,
                            meses = formulario.Pet.Meses,
                            sexo = formulario.Pet.Sexo ?? "Não informado",
                            porte = formulario.Pet.Porte ?? "Não informado",
                            imagem = formulario.Pet.NomeArquivoImagem ?? ""
                        },
                        
                        usuario = new {
                            id = formulario.Usuario.Id,
                            nome = formulario.Usuario.Nome ?? "Não informado",
                            email = formulario.Usuario.Email ?? "Não informado",
                            telefone = formulario.Usuario.Telefone ?? "Não informado",
                            cpf = formulario.Usuario.CPF ?? "Não informado",
                            logradouro = formulario.Usuario.Logradouro ?? "Não informado",
                            numero = formulario.Usuario.Numero ?? "S/N",
                            complemento = formulario.Usuario.Complemento ?? "",
                            bairro = formulario.Usuario.Bairro ?? "Não informado",
                            cidade = formulario.Usuario.Cidade ?? "Não informada",
                            estado = formulario.Usuario.Estado ?? "Não informado",
                            uf = formulario.Usuario.Estado ?? "Não informado",
                            cep = formulario.Usuario.CEP ?? "Não informado"
                        }
                    });
                }
                
                if (adocao.Pet == null || adocao.Usuario == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Dados incompletos da adoção" });
                }
                
                return Json(new {
                    id = adocao.Id,
                    dataEnvio = adocao.DataEnvio,
                    dataPedido = adocao.DataEnvio,
                    dataResposta = adocao.DataResposta,
                    dataFinalizacao = adocao.DataFinalizacao,
                    dataCancelamento = adocao.Status != null && adocao.Status.Contains("Cancelado") ? adocao.DataFinalizacao : null,
                    status = adocao.Status ?? "Pendente",
                    motivoCancelamento = adocao.ObservacoesCancelamento,
                    observacoesCancelamento = adocao.ObservacoesCancelamento,
                    
                    pet = new {
                        id = adocao.Pet.Id,
                        nome = adocao.Pet.Nome ?? "Não informado",
                        especie = adocao.Pet.Especie ?? "Não informada",
                        raca = adocao.Pet.Raca ?? "Não informada",
                        idade = (adocao.Pet.Anos > 0 ? $"{adocao.Pet.Anos} anos" : "") + 
                               (adocao.Pet.Meses > 0 ? $" e {adocao.Pet.Meses} meses" : ""),
                        anos = adocao.Pet.Anos,
                        meses = adocao.Pet.Meses,
                        sexo = adocao.Pet.Sexo ?? "Não informado",
                        porte = adocao.Pet.Porte ?? "Não informado",
                        imagem = adocao.Pet.NomeArquivoImagem ?? ""
                    },
                    
                    usuario = new {
                        id = adocao.Usuario.Id,
                        nome = adocao.Usuario.Nome ?? "Não informado",
                        email = adocao.Usuario.Email ?? "Não informado",
                        telefone = adocao.Usuario.Telefone ?? "Não informado",
                        cpf = adocao.Usuario.CPF ?? "Não informado",
                        logradouro = adocao.Usuario.Logradouro ?? "Não informado",
                        numero = adocao.Usuario.Numero ?? "S/N", 
                        complemento = adocao.Usuario.Complemento ?? "",
                        bairro = adocao.Usuario.Bairro ?? "Não informado",
                        cidade = adocao.Usuario.Cidade ?? "Não informada",
                        estado = adocao.Usuario.Estado ?? "Não informado",
                        uf = adocao.Usuario.Estado ?? "Não informado",
                        cep = adocao.Usuario.CEP ?? "Não informado"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { sucesso = false, mensagem = $"Erro ao obter detalhes: {ex.Message}" });
            }
        }

        [HttpGet("StatusAtualAdocao/{id}")]
        public async Task<IActionResult> StatusAtualAdocao(int id)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .FirstOrDefaultAsync(a => a.Id == id);
                    
                if (adocao == null)
                {
                    return NotFound();
                }
                
                return Json(new { status = adocao.Status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpPost("atualizarStatus/{adocaoId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarStatusAdocao(int adocaoId, string novoStatus, string observacoes = "")
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == adocaoId);

                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada" });
                }

                var statusAntigo = adocao.Status;
                adocao.Status = novoStatus;

                if (novoStatus == "Finalizada" || novoStatus == "Cancelada")
                {
                    adocao.DataFinalizacao = DateTime.Now;
                }

                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    switch (novoStatus)
                    {
                        case "Finalizada":
                            pet.Status = "Adotado";
                            break;
                        case "Cancelada":
                            pet.Status = "Disponível";
                            break;
                        case "Aprovado":
                            pet.Status = "Em Processo";
                            break;
                        case "Aguardando buscar":
                            pet.Status = "Em Processo";
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(observacoes) && (novoStatus == "Cancelada"))
                {
                    adocao.ObservacoesCancelamento = observacoes;
                }

                var formulario = await _contexto.FormulariosAdocao
                    .FirstOrDefaultAsync(f => f.PetId == adocao.PetId && f.UsuarioId == adocao.UsuarioId);
                    
                if (formulario != null)
                {
                    formulario.Status = novoStatus;
                    
                    if (formulario.DataResposta == null && 
                        (novoStatus != "Pendente" && novoStatus != statusAntigo))
                    {
                        formulario.DataResposta = DateTime.Now;
                    }
                    
                    if (!string.IsNullOrWhiteSpace(observacoes))
                    {
                        if (novoStatus == "Cancelada")
                        {
                            formulario.ObservacoesCancelamento = observacoes;
                        }
                        else
                        {
                            formulario.ObservacaoAdminFormulario = observacoes;
                        }
                    }
                }

                if (adocao.Usuario != null)
                {
                    string titulo = $"Status da adoção atualizado";
                    string mensagem = $"O status da adoção do pet {adocao.Pet?.Nome} foi atualizado para '{novoStatus}'.";
                    
                    if (!string.IsNullOrWhiteSpace(observacoes))
                    {
                        mensagem += $" Observações: {observacoes}";
                    }
                    
                    await _servicoNotificacao.CriarNotificacao(
                        adocao.UsuarioId.ToString(),
                        titulo,
                        mensagem,
                        "Adocao",
                        adocao.Id.ToString()
                    );
                }

                await _contexto.SaveChangesAsync();

                return Json(new { sucesso = true, mensagem = $"Status atualizado para '{novoStatus}'" });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao atualizar status: {ex.Message}" });
            }
        }

        [HttpPost("Cancelar/{id}")]
        public async Task<IActionResult> CancelarAdocaoAjax(int id, string motivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motivo) || motivo.Length < 5)
                {
                    return Json(new { sucesso = false, mensagem = "Por favor, informe um motivo de cancelamento com pelo menos 5 caracteres." });
                }
                
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == id);
                    
                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = "Adoção não encontrada." });
                }
                
                if (adocao.Status == "Finalizada" || adocao.Status == "Cancelada")
                {
                    return Json(new { 
                        sucesso = false, 
                        mensagem = $"Não é possível cancelar uma adoção que já está {adocao.Status.ToLower()}." 
                    });
                }
                
                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Usuario)
                    .FirstOrDefaultAsync(f => f.PetId == adocao.PetId && f.UsuarioId == adocao.UsuarioId);
                    
                var observacaoCancelamento = $"Cancelado pelo Administrador: {motivo}";
                
                adocao.Status = "Cancelado pelo Admin";
                adocao.DataFinalizacao = DateTime.Now;
                adocao.ObservacoesCancelamento = observacaoCancelamento;
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    pet.Status = "Disponível";
                }
                
                if (formulario != null)
                {
                    formulario.ObservacoesCancelamento = observacaoCancelamento;
                }
                
                try
                {
                    if (adocao.Usuario != null)
                    {
                        await _servicoNotificacao.CriarNotificacao(
                            adocao.UsuarioId.ToString(),
                            "Adoção cancelada",
                            $"A adoção do pet {adocao.Pet?.Nome} foi cancelada pelo administrador. Motivo: {motivo}",
                            "Adocao",
                            adocao.Id.ToString()
                        );
                    }
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"Erro ao criar notificação: {notifEx.Message}");
                }
                
                await _contexto.SaveChangesAsync();
                
                return Json(new { 
                    sucesso = true, 
                    mensagem = "Adoção cancelada com sucesso!",
                    dataCancelamento = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    observacoes = observacaoCancelamento
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao cancelar adoção: {ex.Message}" });
            }
        }

        [HttpGet("Contadores")]
        public async Task<IActionResult> ContadoresAdocao()
        {
            try
            {
                var totalGeral = await _contexto.Adocoes.CountAsync();
                var emAndamento = await _contexto.Adocoes.CountAsync(a => a.Status == "Aprovado" || a.Status == "Aguardando buscar");
                var finalizadas = await _contexto.Adocoes.CountAsync(a => a.Status == "Finalizada");
                var canceladas = await _contexto.Adocoes.CountAsync(a => a.Status == "Cancelada" || a.Status == "Rejeitada");
                
                return Json(new {
                    totalGeral,
                    emAndamento,
                    finalizadas,
                    canceladas
                });
            }
            catch (Exception ex)
            {
                return Json(new { erro = ex.Message });
            }
        }

        [HttpGet("BuscarHistoricoAdocoes")]
        public async Task<IActionResult> BuscarHistoricoAdocoes(int usuarioId)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
                
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuário não encontrado" });
                }
                
                var historico = await _contexto.Adocoes
                    .Where(a => a.UsuarioId == usuarioId)
                    .OrderByDescending(a => a.DataEnvio)
                    .Select(a => new 
                    {
                        id = a.Id,
                        petId = a.PetId,
                        nomePet = a.Pet.Nome,
                        imagemPet = "/imagens/pets/" + a.Pet.NomeArquivoImagem,
                        dataEnvio = a.DataEnvio,
                        dataResposta = a.DataResposta,
                        dataFinalizacao = a.DataFinalizacao,
                        status = a.Status
                    })
                    .ToListAsync();
                
                return Json(new { success = true, adocoes = historico });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao buscar histórico: {ex.Message}" });
            }
        }

        [HttpGet("HistoricoAdocoes")]
        public async Task<IActionResult> HistoricoAdocoes(int usuarioId)
        {
            return await BuscarHistoricoAdocoes(usuarioId);
        }

        [HttpGet("HistoricoAdocoes/{id}")]
        public async Task<IActionResult> HistoricoAdocoesComId(int id)
        {
            return await BuscarHistoricoAdocoes(id);
        }

        [HttpPost("FinalizarAdocao")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarAdocaoAjax([FromBody] JsonDocument modelDocument)
        {
            try
            {
                if (modelDocument == null)
                {
                    return Json(new { sucesso = false, mensagem = "Nenhum dado recebido" });
                }
                
                var root = modelDocument.RootElement;
                
                if (!root.TryGetProperty("id", out var idElement))
                {
                    return Json(new { sucesso = false, mensagem = "ID da adoção não fornecido" });
                }
                
                int id = idElement.GetInt32();
                string observacao = string.Empty;
                
                if (root.TryGetProperty("observacao", out var obsElement) && !obsElement.ValueKind.Equals(JsonValueKind.Null)) 
                {
                    observacao = obsElement.GetString() ?? string.Empty;
                }
                
                if (id <= 0)
                {
                    return Json(new { sucesso = false, mensagem = "ID inválido fornecido" });
                }
                
                Console.WriteLine($"Finalizando adoção ID: {id}, Observação: {observacao}");
                
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .FirstOrDefaultAsync(a => a.Id == id);
                    
                if (adocao == null)
                {
                    return Json(new { sucesso = false, mensagem = $"Adoção com ID {id} não encontrada" });
                }
                
                Console.WriteLine($"Adoção encontrada: PetId={adocao.PetId}, UsuarioId={adocao.UsuarioId}, Status={adocao.Status}");
                
                if (adocao.Status == "Finalizada")
                {
                    return Json(new { sucesso = false, mensagem = "Esta adoção já está finalizada" });
                }
                
                if (adocao.Status == "Cancelada")
                {
                    return Json(new { sucesso = false, mensagem = "Não é possível finalizar uma adoção cancelada" });
                }
                
                adocao.Status = "Finalizada";
                adocao.DataFinalizacao = DateTime.Now;
                
                
                var pet = await _contexto.Pets.FindAsync(adocao.PetId);
                if (pet != null)
                {
                    Console.WriteLine($"Atualizando pet ID {pet.Id} para status 'Adotado'");
                    pet.Status = "Adotado";
                }
                else
                {
                    Console.WriteLine($"AVISO: Pet ID {adocao.PetId} não encontrado");
                }
                
                var formulario = await _contexto.FormulariosAdocao
                    .FirstOrDefaultAsync(f => f.PetId == adocao.PetId && f.UsuarioId == adocao.UsuarioId);
                    
                if (formulario != null)
                {
                    Console.WriteLine($"Atualizando formulário ID {formulario.Id}");
                    formulario.Status = "Finalizada";
                    formulario.DataResposta = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(observacao))
                    {
                        formulario.ObservacaoAdminFormulario = observacao;
                    }
                }
                
                try 
                {
                    if (adocao.Usuario != null)
                    {
                        var usuarioId = adocao.Usuario.CPF;
                        var petNome = adocao.Pet?.Nome ?? "pet";
                        
                        if (!string.IsNullOrEmpty(usuarioId))
                        {
                            Console.WriteLine($"Criando notificação para usuário {usuarioId}");
                            await _servicoNotificacao.CriarNotificacao(
                                usuarioId,
                                "Adoção finalizada com sucesso",
                                $"A adoção do pet {petNome} foi finalizada com sucesso! Obrigado por adotar um amigo.",
                                "Adocao",
                                adocao.Id.ToString()
                            );
                        }
                        else
                        {
                            Console.WriteLine("Não foi possível criar notificação: ID de usuário inválido");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Não foi possível criar notificação: Usuário não encontrado");
                    }
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"Erro ao criar notificação: {notifEx.Message}");
                }
                
                await _contexto.SaveChangesAsync();
                
                Console.WriteLine("Adoção finalizada com sucesso");
                return Json(new { sucesso = true, mensagem = "Adoção finalizada com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Erro de operação inválida: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { sucesso = false, mensagem = $"Operação inválida: {ex.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao finalizar adoção: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                
                return Json(new { sucesso = false, mensagem = $"Erro ao finalizar adoção: {ex.Message}" });
            }
        }

        [HttpGet("avaliar-formulario/{id}")]
        public async Task<IActionResult> AvaliarFormularioAdocao(int id)
        {
            var formulario = await _contexto.FormulariosAdocao
                .Include(f => f.Pet)
                .Include(f => f.Usuario)
                .FirstOrDefaultAsync(f => f.Id == id);
                
            if (formulario == null)
            {
                return NotFound();
            }
            
            var viewModel = new AvaliacaoAdocaoViewModel
            {
                Id = formulario.Id,
                PetId = formulario.PetId,
                UsuarioId = formulario.UsuarioId,
                Status = formulario.Status,
                DataEnvio = formulario.DataEnvio,
                DataResposta = formulario.DataResposta,
                ObservacaoAdminFormulario = formulario.ObservacaoAdminFormulario
            };
            
            return View(viewModel);
        }

        [HttpGet("ObterDadosUsuario")]
        public async Task<IActionResult> ObterDadosUsuario(int id)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(id);
                
                if (usuario == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Usuário não encontrado" });
                }
                
                var resultado = new
                {
                    sucesso = true,
                    dados = new
                    {
                        id = usuario.Id,
                        nomeCompleto = usuario.Nome,
                        email = usuario.Email,
                        cpf = usuario.CPF,
                        telefone = usuario.Telefone,
                        dataNascimento = usuario.DataNascimento,
                        endereco = usuario.Logradouro,
                        numero = usuario.Numero,
                        complemento = usuario.Complemento,
                        bairro = usuario.Bairro,
                        cidade = usuario.Cidade,
                        estado = usuario.Estado,
                        cep = usuario.CEP,
                        dataCadastro = usuario.DataCadastro
                    }
                };
                
                return Json(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { sucesso = false, mensagem = $"Erro ao obter detalhes do usuário: {ex.Message}" });
            }
        }

        [HttpGet("ObterHistoricoAdocoesPorUsuario")]
        public async Task<IActionResult> ObterHistoricoAdocoesPorUsuario(int usuarioId)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Usuário não encontrado" });
                }
                
                var adocoes = await _contexto.Adocoes
                    .Include(a => a.Pet)
                    .Where(a => a.UsuarioId == usuarioId)
                    .OrderByDescending(a => a.DataEnvio)
                    .ToListAsync();
                
                var adocoesPetIds = adocoes.Select(a => a.PetId).ToList();
                var formularios = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .Where(f => f.UsuarioId == usuarioId && !adocoesPetIds.Contains(f.PetId))
                    .OrderByDescending(f => f.DataEnvio)
                    .ToListAsync();
                
                var dadosAdocoes = adocoes.Select(a => new 
                {
                    id = a.Id,
                    petId = a.PetId,
                    petNome = a.Pet?.Nome ?? "Pet não encontrado",
                    petImagem = a.Pet?.NomeArquivoImagem,
                    status = a.Status,
                    dataEnvio = a.DataEnvio,
                    dataResposta = a.DataResposta,
                    dataFormatada = a.DataEnvio.ToString("dd/MM/yyyy HH:mm"),
                    tipoRegistro = "Adoção"
                });
                
                var dadosFormularios = formularios.Select(f => new 
                {
                    id = f.Id,
                    petId = f.PetId,
                    petNome = f.Pet?.Nome ?? "Pet não encontrado",
                    petImagem = f.Pet?.NomeArquivoImagem,
                    status = f.Status,
                    dataEnvio = f.DataEnvio,
                    dataResposta = f.DataResposta,
                    dataFormatada = f.DataEnvio.ToString("dd/MM/yyyy HH:mm"),
                    tipoRegistro = "Formulário"
                });
                
                var historico = dadosAdocoes.Concat(dadosFormularios)
                    .OrderByDescending(h => h.dataEnvio)
                    .ToList();
                
                return Json(new { 
                    sucesso = true, 
                    dados = historico,
                    totalRegistros = historico.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter histórico de adoções: {ex.Message}");
                return BadRequest(new { sucesso = false, mensagem = $"Erro ao obter histórico de adoções: {ex.Message}" });
            }
        }

        [HttpGet("ObterEstatisticasAdocaoUsuario")]
        public async Task<IActionResult> ObterEstatisticasAdocaoUsuario(int id)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(id);
                
                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado" });
                }
                
                var todasAdocoes = await _contexto.Adocoes
                    .Where(a => a.UsuarioId == id)
                    .ToListAsync();
                
                var adocoesAprovadas = todasAdocoes.Count(a => a.Status == "Aprovada" || a.Status == "Finalizada" || a.Status == "Aguardando buscar");
                var adocoesRejeitadas = todasAdocoes.Count(a => a.Status == "Rejeitada");
                var adocoesCanceladas = todasAdocoes.Count(a => a.Status.Contains("Cancelad"));
                
                var formularioIds = await _contexto.Adocoes
                    .Where(a => a.UsuarioId == id)
                    .Select(a => a.Id)
                    .ToListAsync();
                
                var formularios = await _contexto.FormulariosAdocao
                    .Where(f => f.UsuarioId == id && !formularioIds.Contains(f.Id))
                    .ToListAsync();
                
                var resultado = new {
                    total = todasAdocoes.Count + formularios.Count,
                    aprovadas = adocoesAprovadas,
                    rejeitadas = adocoesRejeitadas + formularios.Count(f => f.Status == "Rejeitada"),
                    canceladas = adocoesCanceladas + formularios.Count(f => f.Status.Contains("Cancelad")),
                    emAnalise = formularios.Count(f => f.Status == "Pendente" || f.Status == "Em Análise")
                };
                
                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao obter estatísticas: {ex.Message}" });
            }
        }
        
        [HttpGet("ObterHistoricoAdocoesUsuario")]
        public async Task<IActionResult> ObterHistoricoAdocoesUsuario(int id)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(id);
                
                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado" });
                }
                
                var adocoes = await _contexto.Adocoes
                    .Include(a => a.Pet)
                    .Where(a => a.UsuarioId == id)
                    .OrderByDescending(a => a.DataEnvio)
                    .ToListAsync();
                
                var adocoesPetIds = adocoes.Select(a => a.PetId).ToList();
                var formularios = await _contexto.FormulariosAdocao
                    .Include(f => f.Pet)
                    .Where(f => f.UsuarioId == id && !adocoesPetIds.Contains(f.PetId))
                    .OrderByDescending(f => f.DataEnvio)
                    .ToListAsync();
                
                var resultado = new List<object>();
                
                foreach (var adocao in adocoes)
                {
                    resultado.Add(new {
                        id = adocao.Id,
                        nomePet = adocao.Pet?.Nome ?? "Pet não encontrado",
                        imagemPet = adocao.Pet?.NomeArquivoImagem != null ? 
                            $"/imagens/pets/{adocao.Pet.NomeArquivoImagem}" : null,
                        status = adocao.Status,
                        dataAdocao = adocao.DataEnvio,
                        tipoRegistro = "Adoção"
                    });
                }
                
                foreach (var formulario in formularios)
                {
                    resultado.Add(new {
                        id = formulario.Id,
                        nomePet = formulario.Pet?.Nome ?? "Pet não encontrado",
                        imagemPet = formulario.Pet?.NomeArquivoImagem != null ? 
                            $"/imagens/pets/{formulario.Pet.NomeArquivoImagem}" : null,
                        status = formulario.Status,
                        dataAdocao = formulario.DataEnvio,
                        tipoRegistro = "Formulário"
                    });
                }
                
                resultado = resultado.OrderByDescending(r => ((dynamic)r).dataAdocao).ToList();
                
                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao obter histórico: {ex.Message}" });
            }
        }

        [HttpGet("ObterPerfilUsuario")]
        public async Task<IActionResult> ObterPerfilUsuario(int id)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(id);
                
                if (usuario == null)
                {
                    return Json(new { sucesso = false, mensagem = "Usuário não encontrado" });
                }
                
                var resultado = new {
                    id = usuario.Id,
                    nome = usuario.Nome,
                    email = usuario.Email,
                    cpf = usuario.CPF,
                    telefone = usuario.Telefone,
                    dataNascimento = usuario.DataNascimento,
                    logradouro = usuario.Logradouro,
                    numero = usuario.Numero,
                    complemento = usuario.Complemento,
                    bairro = usuario.Bairro,
                    cidade = usuario.Cidade,
                    estado = usuario.Estado,
                    cep = usuario.CEP,
                    fotoPerfil = usuario.FotoPerfil,
                    dataCadastro = usuario.DataCadastro
                };
                
                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao obter perfil: {ex.Message}" });
            }
        }

        [HttpGet("ObterHistoricoAdocoesPet")]
        public async Task<IActionResult> ObterHistoricoAdocoesPet(int petId)
        {
            if (!User.IsInRole("Administrador"))
            {
                return Unauthorized();
            }

            try
            {
                var pet = await _contexto.Pets
                    .FirstOrDefaultAsync(p => p.Id == petId);

                if (pet == null)
                {
                    return Json(new { sucesso = false, mensagem = "Pet não encontrado." });
                }

                var historicoAdocoes = await _contexto.Adocoes
                    .Where(a => a.PetId == petId)
                    .OrderByDescending(a => a.DataResposta)
                    .Select(a => new
                    {
                        status = a.Status,
                        dataResposta = a.DataResposta,
                        observacao = a.ObservacoesCancelamento,
                        petId = pet.Id,
                        petNome = pet.Nome,
                        imagemPet = !string.IsNullOrEmpty(pet.NomeArquivoImagem) ? 
                            $"/imagens/pets/{pet.NomeArquivoImagem}" : null
                    })
                    .ToListAsync();

                return Json(new { sucesso = true, dados = historicoAdocoes });
            }
            catch (Exception)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao processar sua solicitação. Tente novamente mais tarde." });
            }
        }
    }
} 