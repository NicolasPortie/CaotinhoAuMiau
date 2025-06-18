using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Services;
using System.Threading.Tasks;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacaoController : ControllerBase
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificacaoServico _servicoNotificacao;

        public NotificacaoController(ApplicationDbContext contexto, NotificacaoServico servicoNotificacao)
        {
            _contexto = contexto;
            _servicoNotificacao = servicoNotificacao;
        }

        [HttpGet]
        public async Task<IActionResult> ObterNotificacoes()
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized();
            }

            var notificacoes = await _servicoNotificacao.ObterNotificacoesUsuario(idUsuario);
            return Ok(notificacoes);
        }

        [HttpGet("nao-lidas")]
        public async Task<IActionResult> ObterNotificacoesNaoLidas()
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized();
            }

            var notificacoes = await _servicoNotificacao.ContarNotificacoesNaoLidas(idUsuario);
            return Ok(notificacoes);
        }

        [HttpPost("marcar-como-lida/{id}")]
        public async Task<IActionResult> MarcarComoLida(int id)
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized();
            }

            var resultado = await _servicoNotificacao.MarcarComoLida(id, idUsuario);
            if (!resultado)
            {
                return Forbid();
            }
            return Ok();
        }

        [HttpPost("marcar-todas-como-lidas")]
        public async Task<IActionResult> MarcarTodasComoLidas()
        {
            var idUsuario = User.ObterIdUsuario();
            if (string.IsNullOrEmpty(idUsuario))
            {
                return Unauthorized();
            }

            await _servicoNotificacao.MarcarTodasComoLidas(idUsuario);
            return Ok();
        }
    }
} 