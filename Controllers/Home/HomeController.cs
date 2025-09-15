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
using CaotinhoAuMiau.Models.ViewModels.Home;
using System.Diagnostics;
using CaotinhoAuMiau.Models.ViewModels.Comuns;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Models.Enums;
using System.Security.Claims;

namespace CaotinhoAuMiau.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly NotificationService _servicoNotificacao;
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<HomeController> _logger;

        public HomeController(NotificationService servicoNotificacao, ApplicationDbContext contexto, ILogger<HomeController> logger)
        {
            _servicoNotificacao = servicoNotificacao;
            _contexto = contexto;
            _logger = logger;
        }

        private async Task ConfigurarDadosComunsAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var idUsuario = User.ObterIdUsuario();
                if (!string.IsNullOrEmpty(idUsuario))
                {
                    ViewBag.NotificacoesNaoLidas = await _servicoNotificacao.ContarNotificacoesNaoLidasAsync(idUsuario);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated == true;
            var userRole = User?.FindFirst(ClaimTypes.Role)?.Value;
            var allClaims = User?.Claims?.Select(c => $"{c.Type}={c.Value}").ToList();

            _logger.LogInformation("HomeController Index - Auth: {IsAuth}, Role: {Role}, Claims: {Claims}",
                isAuthenticated, userRole, string.Join(", ", allClaims ?? new List<string>()));


            var pets = await _contexto.Pets
                .Where(p => p.Status == StatusPet.Disponivel && p.CadastroCompleto)
                .OrderByDescending(p => p.DataCriacao)
                .Take(6)
                .ToListAsync();

            return View("~/Views/Home/Index.cshtml", pets);
        }

        public async Task<IActionResult> SobreAsync()
        {
            await ConfigurarDadosComunsAsync();
            
            var viewModel = await PrepararViewModelSobre();
            return View("~/Views/Home/Sobre.cshtml", viewModel);
        }

        private async Task<SobreViewModel> PrepararViewModelSobre()
        {
            try
            {
                var statistics = new SobreStatisticsViewModel
                {
                    PetsAdotados = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.Adotado),
                    PetsDisponiveis = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.Disponivel),
                    TotalUsuarios = await _contexto.Usuarios.CountAsync(),
                    Formularios = await _contexto.FormulariosAdocao.CountAsync(),
                    TotalCachorros = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Cao),
                    TotalGatos = await _contexto.Pets.CountAsync(p => p.Especie == Especie.Felino),
                    PetsEmProcesso = await _contexto.Pets.CountAsync(p => p.Status == StatusPet.EmProcesso)
                };

                return new SobreViewModel
                {
                    Statistics = statistics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar estatísticas para página Sobre");
                
                return new SobreViewModel
                {
                    Statistics = new SobreStatisticsViewModel()
                };
            }
        }

        public async Task<IActionResult> ContatoAsync()
        {
            await ConfigurarDadosComunsAsync();
            return View("~/Views/Home/Contato.cshtml");
        }

        public async Task<IActionResult> PrivacidadeAsync()
        {
            await ConfigurarDadosComunsAsync();
            return View("~/Views/Home/Privacidade.cshtml");
        }
        
        public async Task<IActionResult> TermosAsync()
        {
            await ConfigurarDadosComunsAsync();
            return View("~/Views/Home/Termos.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ErroAsync()
        {
            await ConfigurarDadosComunsAsync();
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

    }
}
