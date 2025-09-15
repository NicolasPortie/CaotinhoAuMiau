using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models.ViewModels.Admin;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Models.Enums;
using System.Security.Claims;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Authorize(Roles = "Administrador,Colaborador,Voluntário")]
    [Route("admin/dashboard")]
    public class GerenciamentoDashboardController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<GerenciamentoDashboardController> _logger;

        public GerenciamentoDashboardController(ApplicationDbContext contexto, ILogger<GerenciamentoDashboardController> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> InicioAsync()
        {
            _logger.LogInformation("=== DASHBOARD CONTROLLER CHAMADO ===");
            _logger.LogInformation("URL solicitada: {Url}", HttpContext.Request.Path);
            _logger.LogInformation("Usuário autenticado: {IsAuthenticated}", User?.Identity?.IsAuthenticated);

            if (User?.Identity?.IsAuthenticated == true)
            {
                var userRole = User?.FindFirst(ClaimTypes.Role)?.Value;
                var cargoCustom = User?.FindFirst("Cargo")?.Value;
                var userEmail = User?.FindFirst(ClaimTypes.Email)?.Value;
                var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation("Claims do usuário: Email={Email}, UserId={UserId}, Role={Role}, Cargo={Cargo}",
                    userEmail, userId, userRole, cargoCustom);

                var allClaims = User?.Claims?.Select(c => $"{c.Type}={c.Value}").ToList();
                _logger.LogInformation("Todas as claims: {AllClaims}", string.Join(", ", allClaims ?? new List<string>()));
            }

            var adminId = User.ObterIdUsuario();
            _logger.LogInformation("AdminId extraído: {AdminId}", adminId ?? "NULL");

            if (string.IsNullOrEmpty(adminId))
            {
                _logger.LogWarning("ERRO: Usuário não autenticado tentando acessar dashboard - redirecionando para login");
                return Redirect("/autenticacao/login");
            }

            _logger.LogInformation("Sucesso: Usuário autenticado acessando dashboard - continuando...");
            
            var dashboardViewModel = new DashboardViewModel();
            
            try
            {
                var formularios = await _contexto.FormulariosAdocao
                    .Include(f => f.Usuario)
                    .Include(f => f.Pet)
                    .OrderByDescending(f => f.DataEnvio)
                    .Take(10)
                    .ToListAsync();
                
                dashboardViewModel.Formularios = formularios
                    .Select(f => AdocaoViewModel.Criar(f))
                    .ToList();
                
                dashboardViewModel.Estatisticas.TotalFormularios = await _contexto.FormulariosAdocao.CountAsync();
                dashboardViewModel.Estatisticas.FormulariosPendentes = await _contexto.FormulariosAdocao.CountAsync(f => f.StatusEnum == StatusFormulario.Pendente);
                dashboardViewModel.Estatisticas.FormulariosAprovados = await _contexto.FormulariosAdocao.CountAsync(f => f.StatusEnum == StatusFormulario.Aprovado);
                dashboardViewModel.Estatisticas.FormulariosReprovados = await _contexto.FormulariosAdocao.CountAsync(f => f.StatusEnum == StatusFormulario.Negado);
                
                dashboardViewModel.Estatisticas.TotalPets = await _contexto.Pets.CountAsync();
                dashboardViewModel.Estatisticas.PetsAdotados = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.Adotado);
                dashboardViewModel.Estatisticas.TotalCachorros = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Cao);
                dashboardViewModel.Estatisticas.TotalGatos = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Felino);
                dashboardViewModel.Estatisticas.CachorrosAdotados = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Cao && p.Status == StatusPet.Adotado);
                dashboardViewModel.Estatisticas.GatosAdotados = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Felino && p.Status == StatusPet.Adotado);
                dashboardViewModel.Estatisticas.PetsEmProcesso = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.EmProcesso);
                
                dashboardViewModel.Estatisticas.TotalUsuarios = await _contexto.Usuarios.CountAsync();
                dashboardViewModel.Estatisticas.TotalAdmins = await _contexto.Colaboradores.CountAsync(c => c.Ativo);
                dashboardViewModel.Estatisticas.TotalAdotantes = await _contexto.Usuarios.CountAsync(u => u.Ativo);
                
                dashboardViewModel.Estatisticas.PetsDisponiveis = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.Disponivel);
                dashboardViewModel.Estatisticas.FormulariosPendentesHoje = await _contexto.FormulariosAdocao.CountAsync(f => f.StatusEnum == StatusFormulario.Pendente && f.DataEnvio.Date == DateTime.Today);
                dashboardViewModel.Estatisticas.PetsAguardandoRetirada = await _contexto.Adocoes.CountAsync(a => a.Status == StatusAdocao.AguardandoBuscar);
                dashboardViewModel.Estatisticas.AdocoesFinalizadas = await _contexto.Adocoes.CountAsync(a => a.Status == StatusAdocao.Finalizado);
                
                
                ViewBag.AdocoesRecentes = formularios;
                
                var petsRecentes = await _contexto.Pets
                    .OrderByDescending(p => p.DataCriacao)
                    .Take(5)
                    .ToListAsync();
                
                ViewBag.PetsRecentes = petsRecentes;
                ViewBag.PetsAguardandoRetirada = dashboardViewModel.Estatisticas.PetsAguardandoRetirada;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard");
            }

            return View("~/Views/Admin/GerenciamentoDashboard.cshtml", dashboardViewModel);
        }

        [HttpGet("dados-graficos")]
        public async Task<IActionResult> ObterDadosGraficosAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando carregamento de dados dos gráficos");


                var formulariosPendentes = await _contexto.FormulariosAdocao.CountAsync(f => f.StatusEnum == StatusFormulario.Pendente);
                var formulariosEmAnalise = await _contexto.FormulariosAdocao.CountAsync(f => f.StatusEnum == StatusFormulario.EmAnalise);
                var formulariosAprovados = await _contexto.FormulariosAdocao.CountAsync(f => f.StatusEnum == StatusFormulario.Aprovado);

                var adocoesAguardandoContrato = await _contexto.Adocoes.CountAsync(a => a.Status == StatusAdocao.AguardandoAssinarContrato);
                var adocoesContratoAssinado = await _contexto.Adocoes.CountAsync(a => a.Status == StatusAdocao.ContratoAssinado);
                var adocoesAguardandoRetirada = await _contexto.Adocoes.CountAsync(a => a.Status == StatusAdocao.AguardandoBuscar);
                var adocoesFinalizadas = await _contexto.Adocoes.CountAsync(a => a.Status == StatusAdocao.Finalizado);

                var petsDisponiveis = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.Disponivel);
                var petsAdotados = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.Adotado);
                var petsEmProcesso = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.EmProcesso);
                var petsRascunho = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.Rascunho);

                var cachorrosTotal = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Cao);
                var cachorrosAdotados = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Cao && p.Status == StatusPet.Adotado);
                var gatosTotal = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Felino);
                var gatosAdotados = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Felino && p.Status == StatusPet.Adotado);

                var ultimosDoze = Enumerable.Range(0, 12)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .Select(d => new { Mes = d.Month, Ano = d.Year, Data = d })
                    .OrderBy(d => d.Ano)
                    .ThenBy(d => d.Mes)
                    .ToList();

                var tendenciaFormularios = new List<object>();
                foreach (var periodo in ultimosDoze)
                {
                    var qtdFormularios = await _contexto.FormulariosAdocao
                        .CountAsync(f => f.DataEnvio.Year == periodo.Ano && f.DataEnvio.Month == periodo.Mes);

                    tendenciaFormularios.Add(new
                    {
                        mes = periodo.Data.ToString("MMM/yy"),
                        quantidade = qtdFormularios
                    });
                }

                var resultado = new
                {
                    sucesso = true,
                    timestamp = DateTime.Now,

                    formulariosPendentes,
                    formularios = new
                    {
                        emAnalise = formulariosEmAnalise,
                        aprovados = formulariosAprovados
                    },
                    adocoes = new
                    {
                        aguardandoContrato = adocoesAguardandoContrato,
                        contratoAssinado = adocoesContratoAssinado,
                        aguardandoRetirada = adocoesAguardandoRetirada,
                        finalizadas = adocoesFinalizadas
                    },

                    pets = new
                    {
                        disponiveis = petsDisponiveis,
                        adotados = petsAdotados,
                        emProcesso = petsEmProcesso,
                        rascunho = petsRascunho
                    },

                    especies = new
                    {
                        cachorrosTotal,
                        cachorrosAdotados,
                        gatosTotal,
                        gatosAdotados
                    },

                    tendencia = tendenciaFormularios,

                    debug = new
                    {
                        totalFormularios = formulariosPendentes + formulariosEmAnalise + formulariosAprovados,
                        totalPets = petsDisponiveis + petsAdotados + petsEmProcesso + petsRascunho,
                        totalAdocoes = adocoesAguardandoContrato + adocoesContratoAssinado + adocoesAguardandoRetirada + adocoesFinalizadas,
                        periodoTendencia = $"{ultimosDoze.First().Data:MMM/yyyy} - {ultimosDoze.Last().Data:MMM/yyyy}"
                    }
                };

                _logger.LogInformation("Dados dos gráficos carregados com sucesso. Total formulários: {TotalFormularios}, Total pets: {TotalPets}",
                    resultado.debug.totalFormularios, resultado.debug.totalPets);

                return Json(resultado);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar os dados dos gráficos");
                return Json(new
                {
                    sucesso = false,
                    mensagem = "Erro ao carregar dados dos gráficos.",
                    erro = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }


        [HttpGet("Estatisticas")]
        public IActionResult ExibirEstatisticas()
        {
            return View();
        }

        [HttpGet("AtividadesRecentes")]
        public async Task<IActionResult> ObterAtividadesRecentesAsync()
        {
            try
            {
                var formularios = await _contexto.FormulariosAdocao
                    .Include(f => f.Usuario)
                    .Include(f => f.Pet)
                    .OrderByDescending(f => f.DataEnvio)
                    .Take(3)
                    .ToListAsync();
                    
                var formulariosFormatados = formularios.Select(f => new {
                    Tipo = "formulario",
                    Descricao = $"Formulário de adoção para {f.Pet?.Nome ?? "Pet desconhecido"}",
                    NomeUsuario = f.Usuario?.Nome ?? "Usuário desconhecido",
                    DataOcorrencia = f.DataEnvio,
                    Status = f.StatusEnum.ObterTexto()
                }).ToList();
                    
                var pets = await _contexto.Pets
                    .OrderByDescending(p => p.DataCriacao)
                    .Take(3)
                    .Select(p => new {
                        Tipo = "pet",
                        Descricao = $"Novo pet cadastrado: {p.Nome}",
                        NomeUsuario = "",
                        DataOcorrencia = p.DataCriacao,
                        Status = p.Status.GetEnumMemberValue()
                    })
                    .ToListAsync();
                    
                var usuarios = await _contexto.Usuarios
                    .OrderByDescending(u => u.DataCadastro)
                    .Take(3)
                    .Select(u => new {
                        Tipo = "usuario",
                        Descricao = $"Novo usuário cadastrado: {u.Nome}",
                        NomeUsuario = u.Nome,
                        DataOcorrencia = u.DataCadastro,
                        Status = u.Ativo ? "Ativo" : "Inativo"
                    })
                    .ToListAsync();
                    
                var todasAtividades = formulariosFormatados
                    .Concat(pets)
                    .Concat(usuarios)
                    .OrderByDescending(a => a.DataOcorrencia)
                    .Take(4)
                    .ToList();
                    
                return Json(new { sucesso = true, atividades = todasAtividades });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao buscar atividades recentes.", erro = ex.Message });
            }
        }
    }
} 