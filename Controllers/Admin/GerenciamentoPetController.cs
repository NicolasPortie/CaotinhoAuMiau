using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.ViewModels;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using CaotinhoAuMiau.Models.Enums;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Constants;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Route("admin/pets")]
    [Authorize(Roles = "Administrador,Colaborador,Voluntário")]
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
    [RequestSizeLimit(52428800)]
    public class GerenciamentoPetController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly IWebHostEnvironment _ambiente;
        private readonly NotificationService _servicoNotificacao;
        private readonly IPetService _petService;
        private readonly ILogger<GerenciamentoPetController> _logger;
        private readonly IAuditoriaService _auditoriaService;

        public GerenciamentoPetController(ApplicationDbContext contexto, IWebHostEnvironment ambiente, NotificationService servicoNotificacao, IPetService petService, ILogger<GerenciamentoPetController> logger, IAuditoriaService auditoriaService)
        {
            _contexto = contexto;
            _ambiente = ambiente;
            _servicoNotificacao = servicoNotificacao;
            _petService = petService;
            _logger = logger;
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> ListarAsync(string? filtroNome, Especie? filtroEspecie, StatusPet? filtroStatus, 
            string? ordenarPor = "Nome", string? direcaoOrdem = "Asc", int pagina = 1, int itensPorPagina = 20)
        {
            var adminId = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("ExibirTelaLogin", "Authentication");
            }

            var viewModel = await PrepararViewModelListagem(filtroNome, filtroEspecie, filtroStatus,
                ordenarPor, direcaoOrdem, pagina, itensPorPagina);

            if (!string.IsNullOrEmpty(filtroNome) || filtroEspecie.HasValue || filtroStatus.HasValue || pagina > 1)
            {
                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.FILTRAR_DADOS,
                    "Filtros aplicados na listagem de pets",
                    LogConstants.Categorias.PET,
                    detalhesAdicionais: $"Nome: {filtroNome}, Especie: {filtroEspecie}, Status: {filtroStatus}, Pagina: {pagina}"
                );
            }

            return View("~/Views/Admin/GerenciamentoPet.cshtml", viewModel);
        }

        private async Task<GerenciamentoPetViewModel> PrepararViewModelListagem(string? filtroNome, Especie? filtroEspecie, 
            StatusPet? filtroStatus, string? ordenarPor, string? direcaoOrdem, int pagina, int itensPorPagina)
        {
            pagina = ValidarParametrosPaginacao(pagina, ref itensPorPagina);
            
            var query = ObterQueryBase();
            query = AplicarFiltros(query, filtroNome, filtroEspecie, filtroStatus);
            query = AplicarOrdenacao(query, ordenarPor, direcaoOrdem);
            
            var totalItens = await query.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItens / (double)itensPorPagina));
            
            if (pagina > totalPaginas) pagina = totalPaginas;
            
            var pets = await AplicarPaginacao(query, pagina, itensPorPagina).ToListAsync();
            var adocoes = await ObterAdocoes(pets);
            var statistics = await CalcularEstatisticas();
            
            return CriarViewModel(pets, adocoes, statistics, totalItens, totalPaginas, pagina, itensPorPagina, 
                filtroNome, filtroEspecie, filtroStatus, ordenarPor, direcaoOrdem);
        }

        private static int ValidarParametrosPaginacao(int pagina, ref int itensPorPagina)
        {
            if (pagina < PaginationConstants.FIRST_PAGE) pagina = PaginationConstants.FIRST_PAGE;
            if (itensPorPagina < PaginationConstants.MIN_PAGE_SIZE) itensPorPagina = PaginationConstants.DEFAULT_PAGE_SIZE;
            if (itensPorPagina > PaginationConstants.MAX_PAGE_SIZE) itensPorPagina = PaginationConstants.MAX_PAGE_SIZE;
            return pagina;
        }

        private IQueryable<Pet> ObterQueryBase()
        {
            return _contexto.Pets.AsQueryable();
        }

        private static IQueryable<Pet> AplicarFiltros(IQueryable<Pet> query, string? filtroNome, Especie? filtroEspecie, StatusPet? filtroStatus)
        {
            if (!string.IsNullOrEmpty(filtroNome))
                query = query.Where(p => p.Nome.Contains(filtroNome));

            if (filtroEspecie.HasValue)
                query = query.Where(p => p.Especie == filtroEspecie.Value);

            if (filtroStatus.HasValue)
                query = query.Where(p => p.Status == filtroStatus.Value);

            return query;
        }

        private static IQueryable<Pet> AplicarOrdenacao(IQueryable<Pet> query, string? ordenarPor, string? direcaoOrdem)
        {
            return ordenarPor?.ToLower() switch
            {
                "datacadastro" => direcaoOrdem == "Desc" 
                    ? query.OrderBy(p => p.Status == StatusPet.Rascunho ? 0 : 1).ThenByDescending(p => p.DataCriacao)
                    : query.OrderBy(p => p.Status == StatusPet.Rascunho ? 0 : 1).ThenBy(p => p.DataCriacao),
                "status" => direcaoOrdem == "Desc"
                    ? query.OrderBy(p => p.Status == StatusPet.Rascunho ? 0 : 1).ThenByDescending(p => p.Status)
                    : query.OrderBy(p => p.Status == StatusPet.Rascunho ? 0 : 1).ThenBy(p => p.Status),
                "especie" => direcaoOrdem == "Desc"
                    ? query.OrderBy(p => p.Status == StatusPet.Rascunho ? 0 : 1).ThenByDescending(p => p.Especie)
                    : query.OrderBy(p => p.Status == StatusPet.Rascunho ? 0 : 1).ThenBy(p => p.Especie),
                _ => direcaoOrdem == "Desc"
                    ? query.OrderBy(p => p.Status == StatusPet.Rascunho ? 0 : 1).ThenByDescending(p => p.Nome)
                    : query.OrderBy(p => p.Status == StatusPet.Rascunho ? 0 : 1).ThenBy(p => p.Nome)
            };
        }

        private static IQueryable<Pet> AplicarPaginacao(IQueryable<Pet> query, int pagina, int itensPorPagina)
        {
            return query.Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina);
        }

        private async Task<List<Adocao>> ObterAdocoes(List<Pet> pets)
        {
            try
            {
                var petIds = pets.Select(p => p.Id).ToList();
                
                    var adocoes = await _contexto.Adocoes
                    .Where(a => petIds.Contains(a.PetId))
                    .Include(a => a.Usuario)
                    .Where(a => a.Usuario != null) // Garantir integridade referencial
                    .ToListAsync();
                
                _logger.LogInformation($"Carregadas {adocoes.Count} adoções para {pets.Count} pets");
                return adocoes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar adoções para os pets. Retornando lista vazia.");
                return new List<Adocao>();
            }
        }

        private async Task<PetStatisticsViewModel> CalcularEstatisticas()
        {
            var todasEstatisticas = await _contexto.Pets.ToListAsync();
            return new PetStatisticsViewModel
            {
                TotalPets = todasEstatisticas.Count,
                TotalCachorros = todasEstatisticas.Count(p => p.Especie == Especie.Cao),
                TotalGatos = todasEstatisticas.Count(p => p.Especie == Especie.Felino),
                PetsDisponiveis = todasEstatisticas.Count(p => p.Status == StatusPet.Disponivel),
                PetsAdotados = todasEstatisticas.Count(p => p.Status == StatusPet.Adotado),
                PetsEmProcesso = todasEstatisticas.Count(p => p.Status == StatusPet.EmProcesso),
                PetsRascunho = todasEstatisticas.Count(p => p.Status == StatusPet.Rascunho)
            };
        }

        private GerenciamentoPetViewModel CriarViewModel(List<Pet> pets, List<Adocao> adocoes, PetStatisticsViewModel statistics,
            int totalItens, int totalPaginas, int pagina, int itensPorPagina, string? filtroNome, Especie? filtroEspecie,
            StatusPet? filtroStatus, string? ordenarPor, string? direcaoOrdem)
        {
            return new GerenciamentoPetViewModel
            {
                Pets = pets.Select(p => CriarPetAdminSummaryViewModel(p, adocoes)).ToList(),
                Statistics = statistics,
                FilterOptions = PetFilterOptionsViewModel.Create(),
                Pagination = new PaginationViewModel
                {
                    PaginaAtual = pagina,
                    TotalPaginas = totalPaginas,
                    TotalItens = totalItens,
                    ItensPorPagina = itensPorPagina
                },
                FiltroNome = filtroNome,
                FiltroEspecie = filtroEspecie,
                FiltroStatus = filtroStatus,
                OrdenarPor = ordenarPor,
                DirecaoOrdem = direcaoOrdem
            };
        }

        private static PetAdminSummaryViewModel CriarPetAdminSummaryViewModel(Pet pet, List<Adocao> adocoes)
        {
            var adocao = adocoes.FirstOrDefault(a => a.PetId == pet.Id);
            return new PetAdminSummaryViewModel
            {
                Id = pet.Id,
                Nome = pet.Nome,
                Especie = pet.Especie,
                Raca = pet.Raca ?? "",
                Anos = pet.Anos,
                Meses = pet.Meses,
                Sexo = pet.Sexo.GetEnumMemberValue(),
                Porte = pet.Porte ?? "",
                Descricao = pet.Descricao ?? "",
                Status = pet.Status,
                NomeArquivoImagem = pet.NomeArquivoImagem,
                DataCadastro = pet.DataCriacao,
                DataAdocao = adocao?.DataResposta,
                DataAtualizacao = pet.DataAtualizacao,
                TemAdocaoAtiva = adocao != null,
                NomeAdotante = adocao?.Usuario?.Nome
            };
        }

        [HttpGet("criar")]
        public async Task<IActionResult> ExibirFormularioCriacaoAsync()
        {
            var viewModel = await PrepararViewModelListagem(null, null, null, "Nome", "Asc", 1, 20);
            ViewBag.CriandoPet = true;

            return View("~/Views/Admin/GerenciamentoPet.cshtml", viewModel);
        }

        [HttpPost("SalvarPet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarPetAsync([FromForm]Pet pet, IFormFile? foto, bool RemoverImagem = false, bool CadastroCompleto = true, bool ManterImagemAtual = false)
        {
            if (string.IsNullOrWhiteSpace(pet.Nome))
            {
                TempData["Erro"] = "Nome do pet é obrigatório.";
                return Redirect("/admin/pets");
            }
            
            try
            {
                var resultado = await _petService.SalvarPetAsync(pet, foto, _ambiente.WebRootPath, RemoverImagem, CadastroCompleto, ManterImagemAtual);

                if (resultado.Sucesso)
                {
                    var acao = pet.Id == 0 ? "Criar_Pet" : "Editar_Pet";
                    var descricao = pet.Id == 0 ?
                        $"Pet criado: {pet.Nome} ({pet.Especie})" :
                        $"Pet editado: {pet.Nome} (ID: {pet.Id})";

                    await _auditoriaService.RegistrarAcaoAsync(
                        pet.Id == 0 ? LogConstants.TiposAcao.CADASTRAR_PET : LogConstants.TiposAcao.EDITAR_PET,
                        descricao,
                        LogConstants.Categorias.PET,
                        LogConstants.EntidadesAfetadas.PET,
                        pet.Id == 0 ? null : pet.Id,
                        detalhesAdicionais: $"Espécie: {pet.Especie}, Status: {pet.Status}"
                    );

                    TempData["Sucesso"] = resultado.Mensagem;
                }
                else
                {
                    TempData["Erro"] = resultado.Mensagem;
                }
                
                return Redirect("/admin/pets");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "Erro de banco ao salvar pet.");
                TempData["Erro"] = "Não foi possível salvar o pet devido a um problema de dados.";
                return Redirect("/admin/pets");
            }
            catch (IOException ioEx)
            {
                _logger.LogWarning(ioEx, "Falha de I/O ao processar imagem do pet.");
                TempData["Erro"] = "Erro ao processar a imagem do pet.";
                return Redirect("/admin/pets");
            }
            catch (ArgumentException ex)
            {
                TempData["Erro"] = ex.Message;
                return Redirect("/admin/pets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao salvar pet.");
                throw;
            }
        }


        [HttpPost("SalvarRascunho")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarRascunhoAsync([FromForm]Pet pet, IFormFile? foto)
        {
            if (string.IsNullOrWhiteSpace(pet?.Nome))
            {
                return Json(new { sucesso = false, mensagem = "Nome do pet é obrigatório para salvar como rascunho" });
            }
            
            try
            {
                pet.Status = StatusPet.Rascunho;
                pet.CadastroCompleto = false;
                
                pet.Especie = Especie.Cao;
                pet.Raca = "Não informado";
                pet.Sexo = SexoPet.Macho;
                pet.Porte = "Médio";
                pet.Descricao = pet.Descricao ?? "";
                
                var resultado = await _petService.SalvarPetAsync(pet, foto, _ambiente.WebRootPath, false, false, false);
                
                if (resultado.Sucesso)
                {
                    await _auditoriaService.RegistrarAcaoAsync(
                        LogConstants.TiposAcao.CADASTRAR_PET,
                        $"Rascunho de pet salvo: {pet.Nome}",
                        LogConstants.Categorias.PET,
                        LogConstants.EntidadesAfetadas.PET,
                        pet.Id,
                        detalhesAdicionais: "Status: Rascunho"
                    );
                    return Json(new { sucesso = true, mensagem = "Rascunho salvo com sucesso!" });
                }
                else
                {
                    return Json(new { sucesso = false, mensagem = resultado.Mensagem });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar rascunho");
                return Json(new { sucesso = false, mensagem = $"Erro interno ao salvar rascunho: {ex.Message}" });
            }
        }

        [HttpGet("editar/{id}")]
        public async Task<IActionResult> ExibirFormularioEdicaoAsync(int id)
        {
            var pet = await _contexto.Pets
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null)
            {
                TempData["Erro"] = "Pet não encontrado.";
                return Redirect("/admin/pets");
            }

            var viewModel = await PrepararViewModelListagem(null, null, null, "Nome", "Asc", 1, 20);
            ViewBag.EditandoPet = true;
            ViewBag.PetParaEdicao = pet;

            return View("~/Views/Admin/GerenciamentoPet.cshtml", viewModel);
        }

        [HttpPost("excluir/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirPetAsync(int id)
        {
            var pet = await _contexto.Pets
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null)
            {
                TempData["Erro"] = "Pet não encontrado.";
                return Redirect("/admin/pets");
            }

            if (pet.Status == StatusPet.Adotado || pet.Status == StatusPet.EmProcesso)
            {
                TempData["Erro"] = "Não é possível excluir um pet que está em processo de adoção ou já foi adotado.";
                return Redirect("/admin/pets");
            }

            try
            {
                if (!string.IsNullOrEmpty(pet.NomeArquivoImagem))
                {
                    ImagemHelper.Remover(_ambiente.WebRootPath, "pets", pet.NomeArquivoImagem);
                }

                _contexto.Pets.Remove(pet);
                await _contexto.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.EXCLUIR_PET,
                    $"Pet excluído: {pet.Nome} (ID: {pet.Id})",
                    LogConstants.Categorias.PET,
                    LogConstants.EntidadesAfetadas.PET,
                    pet.Id,
                    LogConstants.NiveisSeveridade.WARNING,
                    detalhesAdicionais: $"Espécie: {pet.Especie}, Status anterior: {pet.Status}"
                );

                TempData["Sucesso"] = $"Pet '{pet.Nome}' excluído com sucesso!";
                return Redirect("/admin/pets");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "Erro ao excluir pet, possível conflito no banco.");
                TempData["Erro"] = "Não foi possível excluir o pet devido a um problema de dados.";
                return Redirect("/admin/pets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao excluir pet.");
                throw;
            }
        }

        [HttpPost("alterar-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatusPetAsync(int id, [FromBody] JsonElement modelo)
        {
            try
            {
                if (!modelo.TryGetProperty("novoStatus", out JsonElement novoStatusElement))
                {
                    return Json(new { sucesso = false, mensagem = "O status do pet não foi informado." });
                }
                
                string? novoStatus = novoStatusElement.GetString();
                
                if (string.IsNullOrEmpty(novoStatus))
                {
                    return Json(new { sucesso = false, mensagem = "O status do pet não foi informado." });
                }
                
                var pet = await _contexto.Pets.FindAsync(id);
                
                if (pet == null)
                {
                    return Json(new { sucesso = false, mensagem = "Pet não encontrado." });
                }
                
                var statusAnterior = pet.Status;
                pet.Status = EnumExtensions.ParseEnumMemberValue<StatusPet>(novoStatus);
                pet.DataAtualizacao = DateTime.Now;
                
                await _contexto.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.ALTERAR_STATUS_PET,
                    $"Status do pet alterado: {pet.Nome} de {statusAnterior} para {pet.Status}",
                    LogConstants.Categorias.PET,
                    LogConstants.EntidadesAfetadas.PET,
                    pet.Id,
                    LogConstants.NiveisSeveridade.INFO,
                    $"Status anterior: {statusAnterior}, Novo status: {pet.Status}"
                );
                
                return Json(new { sucesso = true, mensagem = $"Status do pet alterado para {novoStatus} com sucesso!" });
            }
            catch (DbUpdateConcurrencyException dbConcEx)
            {
                _logger.LogWarning(dbConcEx, "Conflito de concorrência ao alterar status do pet.");
                return Json(new { sucesso = false, mensagem = "O status do pet já foi alterado por outro usuário." });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "Erro de banco ao alterar status do pet.");
                return Json(new { sucesso = false, mensagem = "Não foi possível alterar o status devido a um problema de dados." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao alterar status do pet.");
                throw;
            }
        }

        [HttpGet("ObterPet/{id}")]
        public async Task<IActionResult> ObterDadosPetAsync(int id)
        {
            try
            {
                var pet = await _contexto.Pets
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (pet == null)
                {
                    return Json(new { sucesso = false, mensagem = "Pet não encontrado." });
                }

                await _auditoriaService.RegistrarAcaoAsync(
                    LogConstants.TiposAcao.VISUALIZAR_DETALHES,
                    $"Dados do pet acessados: {pet.Nome} (ID: {pet.Id})",
                    LogConstants.Categorias.PET,
                    LogConstants.EntidadesAfetadas.PET,
                    pet.Id
                );

                var resultado = new
                {
                    id = pet.Id,
                    nome = pet.Nome,
                    especie = pet.Especie,
                    raca = pet.Raca,
                    anos = pet.Anos,
                    meses = pet.Meses,
                    sexo = pet.Sexo.GetEnumMemberValue(),
                    porte = pet.Porte,
                    status = pet.Status,
                    descricao = pet.Descricao,
                    nomeArquivoImagem = pet.NomeArquivoImagem,
                    cadastroCompleto = pet.CadastroCompleto,
                    dataCriacao = pet.DataCriacao,
                    dataAtualizacao = pet.DataAtualizacao
                };
                
                return Json(new { sucesso = true, dados = resultado });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "Erro de banco ao obter dados do pet.");
                return Json(new { sucesso = false, mensagem = "Não foi possível carregar os dados do pet." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao obter pet.");
                throw;
            }
        }

        [HttpPost("CadastrarPet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarPetAjaxAsync(PetViewModel? modelo, IFormFile? imagemUpload)
        {
            try
            {
                _logger.LogInformation("=========== INÍCIO DO LOG DE CADASTRO DE PET ===========");
                _logger.LogInformation("Recebendo cadastro de pet: {Nome}, Espécie: {Especie}", modelo?.Nome, modelo?.Especie);
                _logger.LogInformation("É rascunho: {Rascunho}", modelo?.CadastroCompleto == false);
                _logger.LogInformation("Imagem recebida: {InfoImagem}", imagemUpload != null ? $"Sim, nome: {imagemUpload.FileName}, tamanho: {imagemUpload.Length} bytes" : "Não");
                
                if (modelo == null)
                {
                    return Json(new { sucesso = false, erros = new Dictionary<string, string> { { "Geral", "Dados do pet não fornecidos." } } });
                }

                if (!ModelState.IsValid)
                {
                    var erros = ModelState.Where(ms => ms.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.First().ErrorMessage);
                    return Json(new { sucesso = false, erros });
                }

                var pet = new Pet
                {
                    Nome = modelo.Nome,
                    Especie = modelo.Especie ?? Especie.Cao,
                    Raca = modelo.Raca,
                    Anos = modelo.Anos,
                    Meses = modelo.Meses,
                    Sexo = modelo.Sexo ?? SexoPet.Macho,
                    Porte = modelo.Porte,
                    Descricao = modelo.Descricao,
                    Status = modelo.CadastroCompleto ? StatusPet.Disponivel : StatusPet.Rascunho,
                    DataCriacao = DateTime.Now,
                    CadastroCompleto = modelo.CadastroCompleto,
                    UsuarioId = 0,
                    NomeArquivoImagem = null
                };
                
                if (imagemUpload != null && imagemUpload.Length > 0)
                {
                    try
                    {
                        pet.NomeArquivoImagem = await ImagemHelper.SalvarAsync(
                            imagemUpload,
                            _ambiente.WebRootPath,
                            "pets");
                    }
                    catch (ArgumentException ex)
                    {
                        return Json(new { sucesso = false, mensagem = ex.Message });
                    }
                }
                else
                {
                    pet.NomeArquivoImagem = null;
                }
                
                _contexto.Pets.Add(pet);
                await _contexto.SaveChangesAsync();

                await _auditoriaService.RegistrarAcaoAsync(
                    modelo.CadastroCompleto ? "CADASTRO_PET" : "RASCUNHO_PET",
                    $"Pet {(modelo.CadastroCompleto ? "cadastrado" : "salvo como rascunho")}: {pet.Nome}",
                    "Pet",
                    "Pet",
                    pet.Id,
                    "Info",
                    $"Espécie: {pet.Especie}, Status: {pet.Status}"
                );
                
                return Json(new { 
                    sucesso = true, 
                    mensagem = modelo.CadastroCompleto ? "Pet cadastrado com sucesso!" : "Rascunho salvo com sucesso!",
                    pet = new { 
                        id = pet.Id, 
                        nome = pet.Nome,
                        nomeArquivoImagem = pet.NomeArquivoImagem,
                        cadastroCompleto = pet.CadastroCompleto
                    } 
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "Erro de banco ao cadastrar pet.");
                return Json(new { sucesso = false, mensagem = "Não foi possível cadastrar o pet por um problema de dados." });
            }
            catch (IOException ioEx)
            {
                _logger.LogWarning(ioEx, "Falha de I/O ao salvar imagem do pet.");
                return Json(new { sucesso = false, mensagem = "Erro ao salvar a imagem do pet." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao cadastrar pet.");
                throw;
            }
        }

        [HttpGet("ObterDetalhesPet/{id}")]
        public async Task<IActionResult> ObterDetalhesPetAsync(int id)
        {
            try
            {
                var pet = await _contexto.Pets.FirstOrDefaultAsync(p => p.Id == id);
                
                if (pet == null)
                {
                    return Json(new { success = false, message = "Pet não encontrado" });
                }
                
                string idadeFormatada = $"{pet.Anos} ano(s) e {pet.Meses} mês(es)";
                
                var petDto = new
                {
                    id = pet.Id,
                    nome = pet.Nome,
                    especie = pet.Especie,
                    raca = pet.Raca,
                    sexo = pet.Sexo.GetEnumMemberValue(),
                    porte = pet.Porte,
                    status = pet.Status.GetEnumMemberValue(),
                    idade = idadeFormatada,
                    anos = pet.Anos,
                    meses = pet.Meses,
                    descricao = pet.Descricao,
                    nomeArquivoImagem = pet.NomeArquivoImagem,
                    cadastroCompleto = pet.CadastroCompleto
                };
                
                return Json(new { success = true, pet = petDto });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogWarning(dbEx, "Erro de banco ao obter detalhes do pet.");
                return Json(new { success = false, message = "Não foi possível obter detalhes do pet." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao obter detalhes do pet.");
                throw;
            }
        }

        [HttpGet("ObterListaPets")]
        public async Task<IActionResult> ObterListaPetsAsync()
        {
            try
            {
                var adminId = User.ObterIdUsuario();
                if (string.IsNullOrEmpty(adminId))
                {
                    return Json(new { success = false, message = "Usuário não autenticado" });
                }
                
                var pets = await _contexto.Pets
                    .OrderByDescending(p => p.CadastroCompleto == false)
                    .ThenByDescending(p => p.DataCriacao)
                    .ToListAsync();

                var petsFormatados = pets.Select(p => new
                {
                    p.Id,
                    p.Nome,
                    p.Especie,
                    p.Raca,
                    p.Sexo,
                    p.Porte,
                    p.Anos,
                    p.Meses,
                    p.Status,
                    p.Descricao,
                    p.NomeArquivoImagem,
                    DataCadastro = p.DataCriacao.ToString("dd/MM/yyyy"),
                    p.CadastroCompleto
                });
                
                return Json(new { sucesso = true, pets = petsFormatados });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = $"Erro ao obter pets: {ex.Message}" });
            }
        }


        [HttpGet("verificar-nome")]
        public async Task<IActionResult> VerificarNomeAsync(string nome, int id = 0)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return Json(new { disponivel = false, mensagem = "O nome do pet é obrigatório." });
            }
            
            var nomeTrim = nome.Trim();
            var petExistente = await _contexto.Pets
                .Where(p => p.Nome.ToLower() == nomeTrim.ToLower()
                         && p.Id != id
                         && p.Status != StatusPet.Adotado)
                .FirstOrDefaultAsync();
            
            return Json(new { 
                disponivel = petExistente == null,
                mensagem = petExistente == null ? "" : $"Este nome já está sendo usado por outro pet ativo no sistema."
            });
        }

    }
} 