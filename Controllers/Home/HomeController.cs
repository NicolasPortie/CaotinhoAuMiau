using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Services;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.ViewModels;
using System.Diagnostics;
using CaotinhoAuMiau.Models.ViewModels.Comuns;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly NotificacaoServico _servicoNotificacao;
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<HomeController> _logger;

        public HomeController(NotificacaoServico servicoNotificacao, ApplicationDbContext contexto, ILogger<HomeController> logger)
        {
            _servicoNotificacao = servicoNotificacao;
            _contexto = contexto;
            _logger = logger;
        }

        private async Task ConfigurarDadosComuns()
        {
            if (User.Identity.IsAuthenticated)
            {
                var idUsuario = User.ObterIdUsuario();
                if (!string.IsNullOrEmpty(idUsuario))
                {
                    ViewBag.NotificacoesNaoLidas = await _servicoNotificacao.ContarNotificacoesNaoLidas(idUsuario);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Administrador"))
                {
                    return RedirectToAction("Inicio", "GerenciamentoDashboard");
                }
                return Redirect("/usuario/pets/explorar");
            }

            var pets = await _contexto.Pets
                .Where(p => p.Status == "Disponível" && p.CadastroCompleto)
                .OrderByDescending(p => p.DataCriacao)
                .Take(6)
                .ToListAsync();

            return View("~/Views/Home/Index.cshtml", pets);
        }

        public async Task<IActionResult> Sobre()
        {
            await ConfigurarDadosComuns();
            return View("~/Views/Home/Sobre.cshtml");
        }

        public async Task<IActionResult> Contato()
        {
            await ConfigurarDadosComuns();
            return View("~/Views/Home/Contato.cshtml");
        }

        public async Task<IActionResult> Privacidade()
        {
            await ConfigurarDadosComuns();
            return View("~/Views/Home/Privacidade.cshtml");
        }
        
        public async Task<IActionResult> Termos()
        {
            await ConfigurarDadosComuns();
            return View("~/Views/Home/Termos.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Erro()
        {
            await ConfigurarDadosComuns();
            return View("~/Views/Shared/Error.cshtml", new ErroViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [HttpGet("popular-banco")]
        [AllowAnonymous]
        public async Task<IActionResult> PopularBancoDados()
        {
            try 
            {
                _logger.LogInformation("Iniciando população do banco de dados com pets para teste");
                
                if (await _contexto.Pets.AnyAsync())
                {
                    _logger.LogInformation("Banco de dados já possui pets. Redirecionando para página de pets");
                    return Redirect("/usuario/pets/explorar");
                }
                

                var pets = new List<Pet>
                {
                    new Pet
                    {
                        Nome = "Rex",
                        Especie = "Cachorro",
                        Raca = "Labrador",
                        Anos = 2,
                        Meses = 3,
                        Sexo = "Macho",
                        Porte = "Grande",
                        Descricao = "Rex é um cachorro muito brincalhão e carinhoso, adora crianças e outros animais.",
                        Status = "Disponível",
                        CadastroCompleto = true,
                        NomeArquivoImagem = "labrador.jpg",
                        UsuarioId = null,
                        DataCriacao = DateTime.Now.AddDays(-10)
                    },
                    new Pet
                    {
                        Nome = "Luna",
                        Especie = "Gato",
                        Raca = "Siamês",
                        Anos = 1,
                        Meses = 6,
                        Sexo = "Fêmea",
                        Porte = "Pequeno",
                        Descricao = "Luna é uma gatinha muito dócil e afetuosa, adora ficar no colo e ronronar.",
                        Status = "Disponível",
                        CadastroCompleto = true,
                        NomeArquivoImagem = "siames.jpg",
                        UsuarioId = null,
                        DataCriacao = DateTime.Now.AddDays(-8)
                    },
                    new Pet
                    {
                        Nome = "Bob",
                        Especie = "Cachorro",
                        Raca = "Vira-lata",
                        Anos = 3,
                        Meses = 0,
                        Sexo = "Macho",
                        Porte = "Médio",
                        Descricao = "Bob é um cachorro muito carinhoso e protetor, ótimo para famílias.",
                        Status = "Disponível",
                        CadastroCompleto = true,
                        NomeArquivoImagem = "vira-lata.jpg",
                        UsuarioId = null,
                        DataCriacao = DateTime.Now.AddDays(-6)
                    },
                    new Pet
                    {
                        Nome = "Nina",
                        Especie = "Gato",
                        Raca = "Persa",
                        Anos = 0,
                        Meses = 8,
                        Sexo = "Fêmea",
                        Porte = "Pequeno",
                        Descricao = "Nina é uma gatinha muito brincalhona e curiosa, adora explorar a casa.",
                        Status = "Disponível",
                        CadastroCompleto = true,
                        NomeArquivoImagem = "persa.jpg",
                        UsuarioId = null,
                        DataCriacao = DateTime.Now.AddDays(-4)
                    }
                };
                
                await _contexto.Pets.AddRangeAsync(pets);
                await _contexto.SaveChangesAsync();
                
                _logger.LogInformation($"Foram adicionados {pets.Count} pets ao banco de dados");
                
                return Redirect("/usuario/pets/explorar");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao popular banco de dados");
                return View("Error");
            }
        }
    }
} 