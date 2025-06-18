using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Data;

namespace CaotinhoAuMiau.Controllers.Usuario
{
    [Route("usuario")]
    [Authorize(Roles = "Usuario")]
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _contexto;

        public UsuarioController(ApplicationDbContext contexto)
        {
            _contexto = contexto;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Redirect("/usuario/pets/explorar");
        }

        [HttpGet]
        [Route("adotar/{petId:int}")]
        [AllowAnonymous]
        public IActionResult FormularioAdocao(int petId)
        {
            return RedirectToAction("ExibirFormulario", "Adocao", new { petId });
        }
    }
} 