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
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Authorize(Roles = "Administrador")]
    public class GerenciamentoDashboardController : Controller
    {
        private readonly ApplicationDbContext _contexto;

        public GerenciamentoDashboardController(ApplicationDbContext contexto)
        {
            _contexto = contexto;
        }

        [HttpGet]
        public async Task<IActionResult> Inicio()
        {
            var adminId = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("ExibirTelaLogin", "Autenticacao");
            }
            
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
                dashboardViewModel.Estatisticas.FormulariosPendentes = await _contexto.FormulariosAdocao.CountAsync(f => f.Status == "Pendente");
                dashboardViewModel.Estatisticas.FormulariosAprovados = await _contexto.FormulariosAdocao.CountAsync(f => f.Status == "Aprovado");
                dashboardViewModel.Estatisticas.FormulariosReprovados = await _contexto.FormulariosAdocao.CountAsync(f => f.Status == "Rejeitada");
                
                dashboardViewModel.Estatisticas.TotalPets = await _contexto.Pets.CountAsync();
                dashboardViewModel.Estatisticas.PetsAdotados = await _contexto.Pets.CountAsync(p => p.Status == "Adotado");
                dashboardViewModel.Estatisticas.TotalCachorros = await _contexto.Pets.CountAsync(p => p.Especie == "Cachorro");
                dashboardViewModel.Estatisticas.TotalGatos = await _contexto.Pets.CountAsync(p => p.Especie == "Gato");
                dashboardViewModel.Estatisticas.CachorrosAdotados = await _contexto.Pets.CountAsync(p => p.Especie == "Cachorro" && p.Status == "Adotado");
                dashboardViewModel.Estatisticas.GatosAdotados = await _contexto.Pets.CountAsync(p => p.Especie == "Gato" && p.Status == "Adotado");
                dashboardViewModel.Estatisticas.PetsEmProcesso = await _contexto.Pets.CountAsync(p => p.Status == "Em Processo");
                
                dashboardViewModel.Estatisticas.TotalUsuarios = await _contexto.Usuarios.CountAsync();
                dashboardViewModel.Estatisticas.TotalAdmins = await _contexto.Colaboradores.CountAsync();
                dashboardViewModel.Estatisticas.TotalAdotantes = await _contexto.Usuarios.CountAsync(u => u.Ativo);
                
                dashboardViewModel.Estatisticas.PetsDisponiveis = await _contexto.Pets.CountAsync(p => p.Status == "Disponível");
                dashboardViewModel.Estatisticas.FormulariosPendentesHoje = await _contexto.FormulariosAdocao.CountAsync(f => f.Status == "Pendente" && f.DataEnvio.Date == DateTime.Today);
                dashboardViewModel.Estatisticas.PetsAguardandoRetirada = await _contexto.FormulariosAdocao.CountAsync(f => f.Status == "Aguardando buscar");
                dashboardViewModel.Estatisticas.AdocoesFinalizadas = await _contexto.FormulariosAdocao.CountAsync(f => f.Status == "Finalizada");
                
                await ConfigurarDadosGraficos(dashboardViewModel.Estatisticas);
                
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
                Console.WriteLine($"Erro ao carregar dashboard: {ex.Message}");
            }

            return View("~/Views/Admin/GerenciamentoDashboard.cshtml", dashboardViewModel);
        }
        
        private async Task ConfigurarDadosGraficos(EstatisticasViewModel estatisticas)
        {
            try
            {
                var ultimosDoze = Enumerable.Range(0, 12)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .Select(d => new { Mes = d.Month, Ano = d.Year })
                    .ToList();
                    
                var adocoes = await _contexto.FormulariosAdocao
                    .Where(f => f.Status == "Finalizada" || f.Status == "Aprovado")
                    .Select(f => new { f.Id, Data = f.DataEnvio })
                    .ToListAsync();
                
                var usuarios = await _contexto.Usuarios
                    .Select(u => new { u.Id, Data = u.DataCadastro })
                    .ToListAsync();
                
                estatisticas.MesesAdocoes.Clear();
                estatisticas.AdocoesPorMes.Clear();
                estatisticas.UsuariosPorMes.Clear();
                estatisticas.TotalUsuariosAcumulados.Clear();
                
                foreach (var periodo in ultimosDoze.OrderBy(p => p.Ano).ThenBy(p => p.Mes))
                {
                    var nomeMes = new DateTime(periodo.Ano, periodo.Mes, 1).ToString("MMM");
                    estatisticas.MesesAdocoes.Add(nomeMes);
                    
                    var qtdAdocoes = adocoes.Count(a => a.Data.Year == periodo.Ano && a.Data.Month == periodo.Mes);
                    estatisticas.AdocoesPorMes.Add(qtdAdocoes);
                    
                    var qtdUsuarios = usuarios.Count(u => u.Data.Year == periodo.Ano && u.Data.Month == periodo.Mes);
                    estatisticas.UsuariosPorMes.Add(qtdUsuarios);
                }
                
                int acumulado = 0;
                foreach (var quantidade in estatisticas.UsuariosPorMes)
                {
                    acumulado += quantidade;
                    estatisticas.TotalUsuariosAcumulados.Add(acumulado);
                }
            }
            catch (Exception ex)
            {
                estatisticas.MesesAdocoes.Clear();
                estatisticas.AdocoesPorMes.Clear();
                estatisticas.UsuariosPorMes.Clear();
                estatisticas.TotalUsuariosAcumulados.Clear();
                
                Console.WriteLine($"Erro ao configurar dados dos gráficos: {ex.Message}");
                
                estatisticas.MesesAdocoes.Add("Sem dados");
                estatisticas.AdocoesPorMes.Add(0);
                estatisticas.UsuariosPorMes.Add(0);
                estatisticas.TotalUsuariosAcumulados.Add(0);
            }
        }

        [HttpGet("DadosGraficos")]
        public async Task<IActionResult> ObterDadosGraficos()
        {
            try
            {
                var ultimosDoze = Enumerable.Range(0, 12)
                    .Select(i => DateTime.Now.AddMonths(-i))
                    .Select(d => new { Mes = d.Month, Ano = d.Year })
                    .OrderBy(d => d.Ano)
                    .ThenBy(d => d.Mes)
                    .ToList();
                
                var adocoes = await _contexto.FormulariosAdocao
                    .Where(f => f.Status == "Aprovado" || f.Status == "Finalizada")
                    .Select(f => new { f.Id, DataCriacao = f.DataEnvio })
                    .ToListAsync();

                var adocoesPorMes = new List<object>();
                foreach (var periodo in ultimosDoze)
                {
                    var dataReferencia = new DateTime(periodo.Ano, periodo.Mes, 1);
                    var qtdAdocoes = adocoes.Count(a => a.DataCriacao.Year == periodo.Ano && a.DataCriacao.Month == periodo.Mes);
                    
                    adocoesPorMes.Add(new {
                        Mes = dataReferencia.ToString("MMM/yyyy"),
                        Quantidade = qtdAdocoes
                    });
                }

                var especiesDistribuicao = await _contexto.Pets
                    .GroupBy(p => p.Especie)
                    .Select(g => new { Especie = g.Key, Quantidade = g.Count() })
                    .ToListAsync();

                var statusFormularios = await _contexto.FormulariosAdocao
                    .GroupBy(f => f.Status)
                    .Select(g => new { Status = g.Key, Quantidade = g.Count() })
                    .ToListAsync();
                    
                var usuarios = await _contexto.Usuarios
                    .Select(u => new { u.Id, DataCriacao = u.DataCadastro })
                    .ToListAsync();
                    
                var usuariosPorMes = new List<object>();
                var acumuladoUsuarios = 0;
                var usuariosAcumulados = new List<object>();
                
                foreach (var periodo in ultimosDoze)
                {
                    var dataReferencia = new DateTime(periodo.Ano, periodo.Mes, 1);
                    var qtdUsuarios = usuarios.Count(u => u.DataCriacao.Year == periodo.Ano && u.DataCriacao.Month == periodo.Mes);
                    acumuladoUsuarios += qtdUsuarios;
                    
                    usuariosPorMes.Add(new {
                        Mes = dataReferencia.ToString("MMM/yyyy"),
                        Quantidade = qtdUsuarios
                    });
                    
                    usuariosAcumulados.Add(new {
                        Mes = dataReferencia.ToString("MMM/yyyy"),
                        Quantidade = acumuladoUsuarios
                    });
                }
                
                var statusPets = await _contexto.Pets
                    .GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key, Quantidade = g.Count() })
                    .ToListAsync();

                return Json(new
                {
                    sucesso = true,
                    adocoesPorMes,
                    especiesDistribuicao,
                    statusFormularios,
                    usuariosPorMes,
                    usuariosAcumulados,
                    statusPets
                });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao buscar os dados dos gráficos.", erro = ex.Message });
            }
        }

        [HttpGet("Estatisticas")]
        public IActionResult ExibirEstatisticas()
        {
            return View();
        }

        [HttpGet("AtividadesRecentes")]
        public async Task<IActionResult> ObterAtividadesRecentes()
        {
            try
            {
                var formularios = await _contexto.FormulariosAdocao
                    .Include(f => f.Usuario)
                    .Include(f => f.Pet)
                    .OrderByDescending(f => f.DataEnvio)
                    .Take(3)
                    .Select(f => new {
                        Tipo = "formulario",
                        Descricao = $"Formulário de adoção para {f.Pet.Nome}",
                        NomeUsuario = f.Usuario.Nome,
                        DataOcorrencia = f.DataEnvio,
                        Status = f.Status
                    })
                    .ToListAsync();
                    
                var pets = await _contexto.Pets
                    .OrderByDescending(p => p.DataCriacao)
                    .Take(3)
                    .Select(p => new {
                        Tipo = "pet",
                        Descricao = $"Novo pet cadastrado: {p.Nome}",
                        NomeUsuario = "",
                        DataOcorrencia = p.DataCriacao,
                        Status = p.Status
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
                    
                var todasAtividades = formularios
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