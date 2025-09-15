using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Models.ViewModels;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using CaotinhoAuMiau.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Constants;

namespace CaotinhoAuMiau.Controllers.Usuario
{
    [Route("usuario/pets")]
    public class PetController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificationService _servicoNotificacao;
        private readonly ILogger<PetController> _logger;

        public PetController(ApplicationDbContext contexto, NotificationService servicoNotificacao, ILogger<PetController> logger)
        {
            _contexto = contexto;
            _servicoNotificacao = servicoNotificacao;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ListarAsync()
        {
            var idUsuario = User.ObterIdUsuario();
            if (!string.IsNullOrEmpty(idUsuario))
            {
                ViewBag.NotificacoesNaoLidas = await _servicoNotificacao.ContarNotificacoesNaoLidasAsync(idUsuario);
            }

            var pets = await _contexto.Pets
                .Where(p => p.Status == StatusPet.Disponivel && p.CadastroCompleto)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            var viewModel = PetViewModel.CriarParaListagem();
            viewModel.Pets = pets;
            viewModel.PaginaAtual = 1;
            viewModel.TotalPaginas = 1;
            viewModel.TotalItens = pets.Count;
            viewModel.Especies = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "Cao", Text = "Cao" },
                new SelectListItem { Value = "Felino", Text = "Felino" }
            }, "Value", "Text");
            viewModel.Sexos = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Value = "Macho", Text = "Macho" },
                new SelectListItem { Value = "Fêmea", Text = "Fêmea" }
            }, "Value", "Text");

            return View("~/Views/Usuario/ExplorarPets.cshtml", viewModel);
        }

       
        [HttpGet("explorar")]
        [ActionName("ExplorarPets")]
        public async Task<IActionResult> ExplorarPetsAsync(
            string filtroNome,
            string filtroEspecie, 
            string filtroSexo, 
            string filtroPorte, 
            string filtroIdade,
            string filtroRaca,
            string filtroOrdem = "recentes",
            int itensPorPagina = 12,
            int pagina = 1,
            bool fromForm = false,
            bool navegacaoPagina = false)
        {
            
            var referrer = Request.Headers["Referer"].ToString();
            if (!fromForm && (referrer.Contains("/adocao/formulario/") || referrer.Contains("/usuario/adocao/formulario/")))
            {
                _logger.LogInformation("Redirecionamento detectado do formulário de adoção");
                fromForm = true;
            }
            
            
            try {
                _logger.LogInformation("Acessando ExplorarPets");

                if (itensPorPagina <= 0)
                {
                    itensPorPagina = 12;
                }
                if (itensPorPagina > PaginationConstants.MAX_PAGE_SIZE)
                {
                    itensPorPagina = PaginationConstants.MAX_PAGE_SIZE;
                }

                if (pagina <= 0)
                {
                    pagina = PaginationConstants.FIRST_PAGE;
                }

                _logger.LogInformation($"Usando {itensPorPagina} itens por página, página {pagina}");

                var query = _contexto.Pets.Where(p => p.Status == StatusPet.Disponivel && p.CadastroCompleto);
                
                if (!string.IsNullOrEmpty(filtroNome))
                {
                    query = query.Where(p => p.Nome.Contains(filtroNome));
                }
                
                if (!string.IsNullOrEmpty(filtroEspecie))
                {
                    var especieEnum = EnumExtensions.ParseEnumMemberValue<Especie>(filtroEspecie);
                    query = query.Where(p => p.Especie == especieEnum);
                }

                if (!string.IsNullOrEmpty(filtroSexo))
                {
                    var sexoEnum = EnumExtensions.ParseEnumMemberValue<SexoPet>(filtroSexo);
                    query = query.Where(p => p.Sexo == sexoEnum);
                }

                if (!string.IsNullOrEmpty(filtroPorte))
                {
                    query = query.Where(p => p.Porte == filtroPorte);
                }
                
                if (!string.IsNullOrEmpty(filtroRaca))
                {
                    query = query.Where(p => p.Raca == filtroRaca);
                }

                if (!string.IsNullOrEmpty(filtroIdade))
                {
                    switch (filtroIdade)
                    {
                        case "Filhote":
                            query = query.Where(p => p.Anos < 1);
                            break;
                        case "Adulto":
                            query = query.Where(p => p.Anos >= 1 && p.Anos < 7);
                            break;
                        case "Idoso":
                            query = query.Where(p => p.Anos >= 7);
                            break;
                    }
                }

                switch (filtroOrdem)
                {
                    case "recentes":
                        query = query.OrderByDescending(p => p.DataCriacao);
                        break;
                    case "antigos":
                        query = query.OrderBy(p => p.DataCriacao);
                        break;
                    case "nome":
                        query = query.OrderBy(p => p.Nome);
                        break;
                    case "nome_desc":
                        query = query.OrderByDescending(p => p.Nome);
                        break;
                    default:
                        query = query.OrderByDescending(p => p.DataCriacao);
                        break;
                }

                var totalItens = await query.CountAsync();

                int totalPaginas = (int)Math.Ceiling(totalItens / (double)itensPorPagina);

                if (totalPaginas <= 0)
                {
                    totalPaginas = 1;
                }
                
                if (pagina > totalPaginas && totalPaginas > 0)
                {
                    pagina = totalPaginas;
                }
                
                int skipCount = (pagina - 1) * itensPorPagina;
                
                if (skipCount >= totalItens)
                {
                    skipCount = 0;
                    pagina = 1;
                }
                
                var pets = await query
                    .Skip(skipCount)
                    .Take(itensPorPagina)
                    .ToListAsync();

                var viewModel = new PetViewModel
                {
                    Pets = pets,
                    PaginaAtual = pagina,
                    TotalPaginas = Math.Max(1, totalPaginas),
                    TotalItens = totalItens,
                    FiltroNome = filtroNome,
                    FiltroEspecie = !string.IsNullOrEmpty(filtroEspecie) ? EnumExtensions.ParseEnumMemberValue<Especie>(filtroEspecie) : (Especie?)null,
                    FiltroSexo = !string.IsNullOrEmpty(filtroSexo) ? EnumExtensions.ParseEnumMemberValue<SexoPet>(filtroSexo) : (SexoPet?)null,
                    FiltroPorte = filtroPorte,
                    FiltroIdade = filtroIdade,
                    FiltroRaca = filtroRaca,
                    FiltroOrdem = filtroOrdem,
                    ItensPorPaginaSelecionado = itensPorPagina
                };
                
                viewModel.Especies = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "Cao", Text = "Cao" },
                    new SelectListItem { Value = "Felino", Text = "Felino" }
                }, "Value", "Text");
                
                viewModel.Sexos = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "Macho", Text = "Macho" },
                    new SelectListItem { Value = "Fêmea", Text = "Fêmea" }
                }, "Value", "Text");
                
                viewModel.Portes = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pequeno", Text = "Pequeno" },
                    new SelectListItem { Value = "Médio", Text = "Médio" },
                    new SelectListItem { Value = "Grande", Text = "Grande" }
                }, "Value", "Text");
                
                viewModel.FaixasEtarias = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "Filhote", Text = "Filhote (< 1 ano)" },
                    new SelectListItem { Value = "Adulto", Text = "Adulto (1 a 7 anos)" },
                    new SelectListItem { Value = "Idoso", Text = "Idoso (> 7 anos)" }
                }, "Value", "Text");

                var idUsuario = User.ObterIdUsuario();
                if (!string.IsNullOrEmpty(idUsuario))
                {
                    ViewBag.NotificacoesNaoLidas = await _servicoNotificacao.ContarNotificacoesNaoLidasAsync(idUsuario);
                    
                    var usuario = await _contexto.Usuarios.FindAsync(int.Parse(idUsuario));
                    if (usuario != null && usuario.EmQuarentena && usuario.FimQuarentena.HasValue && DateTime.Now < usuario.FimQuarentena.Value)
                    {
                        ViewBag.EmQuarentena = true;
                        ViewBag.FimQuarentena = usuario.FimQuarentena.Value;
                        ViewBag.MotivoQuarentena = usuario.MotivoQuarentena;
                        ViewBag.DiasRestantesQuarentena = Math.Max(0, (int)(usuario.FimQuarentena.Value - DateTime.Now).TotalDays + 1);
                    }
                    else
                    {
                        ViewBag.EmQuarentena = false;
                    }
                }

                return View("~/Views/Usuario/ExplorarPets.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao acessar ExplorarPets");
                return View("Error");
            }
        }

        [HttpGet("detalhes/{id}")]
        public async Task<IActionResult> DetalhesPetAsync(int id)
        {
            var idUsuario = User.ObterIdUsuario();
            if (!string.IsNullOrEmpty(idUsuario))
            {
                ViewBag.NotificacoesNaoLidas = await _servicoNotificacao.ContarNotificacoesNaoLidasAsync(idUsuario);
            }

            var pet = await _contexto.Pets
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (pet == null)
            {
                return NotFound();
            }
            
            var viewModel = PetViewModel.CriarDaEntidade(pet);
            return View("~/Views/Usuario/DetalhesPet.cshtml", viewModel);
        }

[HttpPost("buscar")]
        public async Task<IActionResult> BuscarPetsAsync(string termoBusca, string especie, string idade, string sexo)
        {
            var petsQuery = _contexto.Pets.AsQueryable();

            petsQuery = petsQuery.Where(p => p.Status == StatusPet.Disponivel && p.CadastroCompleto);

            if (!string.IsNullOrEmpty(termoBusca))
            {
                termoBusca = termoBusca.ToLower();
                petsQuery = petsQuery.Where(p =>
                    p.Nome.ToLower().Contains(termoBusca) ||
                    p.Raca.ToLower().Contains(termoBusca) ||
                    p.Especie.ToString().ToLower().Contains(termoBusca));
            }

            if (!string.IsNullOrEmpty(especie) && especie != "Todos")
            {
                var especieEnum = EnumExtensions.ParseEnumMemberValue<Especie>(especie);
                petsQuery = petsQuery.Where(p => p.Especie == especieEnum);
            }

            if (!string.IsNullOrEmpty(idade))
            {
                switch (idade)
                {
                    case "Filhote":
                        petsQuery = petsQuery.Where(p => p.Anos < 1);
                        break;
                    case "Adulto":
                        petsQuery = petsQuery.Where(p => p.Anos >= 1 && p.Anos < 7);
                        break;
                    case "Idoso":
                        petsQuery = petsQuery.Where(p => p.Anos >= 7);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(sexo) && sexo != "Todos")
            {
                var sexoEnum = EnumExtensions.ParseEnumMemberValue<SexoPet>(sexo);
                petsQuery = petsQuery.Where(p => p.Sexo == sexoEnum);
            }

            var pets = await petsQuery
                .OrderByDescending(p => p.DataCriacao)
                .Select(p => new {
                    p.Id,
                    p.Nome,
                    p.Especie,
                    p.Raca,
                    p.Sexo,
                    Idade = (p.Anos > 0 ? p.Anos + " ano(s) " : "") + (p.Meses > 0 ? p.Meses + " mês(es)" : ""),
                    p.Porte,
                    p.NomeArquivoImagem,
                    ImagemUrl = !string.IsNullOrEmpty(p.NomeArquivoImagem)
                                ? $"/imagens/Imagens CaotinhoAuMiau/{p.NomeArquivoImagem}"
                                : "/imagens/default-pet.jpg"
                })
                .Take(10)
                .ToListAsync();

            return Json(new { sucesso = true, dados = pets });
        }
    }
}

namespace CaotinhoAuMiau.Controllers.API
{
    [Route("api/pet")]
    [ApiController]
    public class PetApiController : ControllerBase
    {
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<PetApiController> _logger;

        public PetApiController(ApplicationDbContext contexto, ILogger<PetApiController> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        [HttpGet("detalhes/{id}")]
        public async Task<IActionResult> ObterDetalhesPetAsync(int id)
        {
            try
            {
                var pet = await _contexto.Pets
                    .Where(p => p.Id == id && p.Status == StatusPet.Disponivel)
                    .Select(p => new
                    {
                        p.Id,
                        p.Nome,
                        p.Especie,
                        p.Sexo,
                        p.Porte,
                        p.Anos,
                        p.Meses,
                        p.Raca,
                        p.Descricao,
                        p.NomeArquivoImagem,
                        p.CadastroCompleto
                    })
                    .FirstOrDefaultAsync();

                if (pet == null)
                {
                    return NotFound(new { erro = "Pet não encontrado" });
                }

                return Ok(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes do pet {PetId}", id);
                return StatusCode(500, new { erro = "Erro interno do servidor" });
            }
        }

    }
} 