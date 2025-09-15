using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Services;
using CaotinhoAuMiau.Models.ViewModels.Admin;

namespace CaotinhoAuMiau.Controllers.Admin
{
    [Authorize]
    [Route("admin/logs")]
    public class GerenciamentoLogsController : Controller
    {
        private readonly ApplicationDbContext _contexto;
        private readonly IAuditoriaService _auditoriaService;

        public GerenciamentoLogsController(ApplicationDbContext contexto, IAuditoriaService auditoriaService)
        {
            _contexto = contexto;
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? dataInicio = null, DateTime? dataFim = null,
            string? tipoAcao = null, string? categoria = null, string? nivelSeveridade = null,
            int pagina = 1, int itensPorPagina = 20)
        {
            var logs = await _auditoriaService.ObterLogsAsync(dataInicio, dataFim, tipoAcao, categoria, nivelSeveridade, pagina, itensPorPagina);
            var totalLogs = await _auditoriaService.ContarLogsAsync(dataInicio, dataFim, tipoAcao, categoria, nivelSeveridade);

            var estatisticas = await _auditoriaService.ObterEstatisticasLogsAsync(dataInicio, dataFim);

            var viewModel = new LogsViewModel
            {
                Logs = logs,
                DataInicio = dataInicio,
                DataFim = dataFim,
                TipoAcaoSelecionado = tipoAcao,
                CategoriaSelecionada = categoria,
                NivelSeveridadeSelecionado = nivelSeveridade,
                PaginaAtual = pagina,
                TotalPaginas = (int)Math.Ceiling((double)totalLogs / itensPorPagina),
                TotalLogs = totalLogs,
                ItensPorPagina = itensPorPagina,

                TiposAcao = await _contexto.Logs.Where(l => l.Ativo)
                    .Select(l => l.TipoAcao)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync(),

                Categorias = await _contexto.Logs.Where(l => l.Ativo)
                    .Select(l => l.Categoria)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),

                NiveisSeveridade = await _contexto.Logs.Where(l => l.Ativo)
                    .Select(l => l.NivelSeveridade)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToListAsync(),

                Estatisticas = estatisticas
            };

            return View("~/Views/Admin/GerenciamentoLogs.cshtml", viewModel);
        }

        [HttpGet("estatisticas")]
        public async Task<IActionResult> ObterEstatisticas(DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var estatisticas = await _auditoriaService.ObterEstatisticasLogsAsync(dataInicio, dataFim);
            return Json(estatisticas);
        }

        [HttpPost("exportar")]
        public async Task<IActionResult> ExportarLogs(DateTime? dataInicio = null, DateTime? dataFim = null,
            string? tipoAcao = null, string? categoria = null, string? nivelSeveridade = null)
        {
            var logs = await _auditoriaService.ObterLogsAsync(dataInicio, dataFim, tipoAcao, categoria, nivelSeveridade, 1, int.MaxValue);

            var csv = "Data/Hora,Usuário,Perfil,Tipo Ação,Categoria,Descrição,Severidade,Entidade,Detalhes\n";

            foreach (var log in logs)
            {
                csv += $"{log.DataHora:dd/MM/yyyy HH:mm:ss}," +
                       $"\"{log.UsuarioNome}\"," +
                       $"\"{log.PerfilUsuario}\"," +
                       $"\"{log.TipoAcao}\"," +
                       $"\"{log.Categoria}\"," +
                       $"\"{log.Descricao}\"," +
                       $"\"{log.NivelSeveridade}\"," +
                       $"\"{log.EntidadeAfetada ?? "N/A"}\"," +
                       $"\"{log.DetalhesAdicionais?.Replace("\"", "\"\"") ?? "N/A"}\"\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            var fileName = $"logs_auditoria_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}