using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CaotinhoAuMiau.Services
{
    public interface IAuditoriaService
    {
        Task RegistrarAcaoAsync(string tipoAcao, string descricao, string categoria = "Sistema",
            string? entidadeAfetada = null, int? entidadeId = null, string nivelSeveridade = "Info",
            string? detalhesAdicionais = null);
        Task RegistrarLoginAsync(string email, bool sucesso, string? detalhes = null, string? nomeUsuario = null, string? perfilUsuario = null);
        Task RegistrarLogoutAsync(string email);
        Task RegistrarTentativaAcessoNegadoAsync(string recurso, string? detalhes = null);
        Task RegistrarExcecaoAsync(Exception ex, string contexto);
        Task<List<Log>> ObterLogsAsync(DateTime? dataInicio = null, DateTime? dataFim = null,
            string? tipoAcao = null, string? categoria = null, string? nivelSeveridade = null,
            int pagina = 1, int itensPorPagina = 50);
        Task<int> ContarLogsAsync(DateTime? dataInicio = null, DateTime? dataFim = null,
            string? tipoAcao = null, string? categoria = null, string? nivelSeveridade = null);
        Task<Dictionary<string, int>> ObterEstatisticasLogsAsync(DateTime? dataInicio = null, DateTime? dataFim = null);
    }

    public class AuditoriaService : IAuditoriaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarAcaoAsync(string tipoAcao, string descricao, string categoria = "Sistema",
            string? entidadeAfetada = null, int? entidadeId = null, string nivelSeveridade = "Info",
            string? detalhesAdicionais = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var user = httpContext?.User;

                string usuarioEmail = "Sistema";
                string usuarioNome = "Sistema";
                string perfilUsuario = "Sistema";
                string? userAgent = null;

                if (user?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                    {
                        var colaborador = await _context.Colaboradores
                            .FirstOrDefaultAsync(c => c.UsuarioId == userId && c.Ativo);

                        if (colaborador != null)
                        {
                            usuarioEmail = colaborador.Email ?? "Email não informado";
                            usuarioNome = !string.IsNullOrWhiteSpace(colaborador.Nome) ? colaborador.Nome : $"Colaborador #{colaborador.Id}";
                            perfilUsuario = colaborador.Cargo.ToString();
                        }
                        else
                        {
                            var usuario = await _context.Usuarios
                                .FirstOrDefaultAsync(u => u.Id == userId && u.Ativo);

                            if (usuario != null)
                            {
                                usuarioEmail = usuario.Email ?? "Email não informado";
                                usuarioNome = !string.IsNullOrWhiteSpace(usuario.Nome) ? usuario.Nome : $"Usuario #{usuario.Id}";
                                perfilUsuario = "Usuario";
                            }
                            else
                            {
                                var emailClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                                var nameClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                                var roleClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                                usuarioEmail = emailClaim ?? user.Identity.Name ?? "email@desconhecido.com";
                                usuarioNome = nameClaim ?? $"Usuario #{userId}";
                                perfilUsuario = roleClaim ?? "Desconhecido";
                            }
                        }
                    }
                    else
                    {
                        var emailClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                        var nameClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                        var roleClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                        usuarioNome = nameClaim ?? "Sistema";
                        usuarioEmail = emailClaim ?? user.Identity.Name ?? "email@desconhecido.com";
                        perfilUsuario = roleClaim ?? "Desconhecido";
                    }
                }
                else
                {
                    usuarioNome = "Sistema";
                    usuarioEmail = "sistema@caotinhoaumiau.com";
                }

                if (httpContext != null)
                {
                    userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
                }

                var log = new Log
                {
                    DataHora = DateTime.Now,
                    UsuarioEmail = usuarioEmail,
                    UsuarioNome = usuarioNome,
                    PerfilUsuario = perfilUsuario,
                    TipoAcao = tipoAcao,
                    Categoria = categoria,
                    Descricao = descricao,
                    EntidadeAfetada = entidadeAfetada,
                    EntidadeId = entidadeId,
                    NivelSeveridade = nivelSeveridade,
                    UserAgent = userAgent,
                    DetalhesAdicionais = detalhesAdicionais,
                    Ativo = true
                };

                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao registrar log de auditoria: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        public async Task RegistrarLoginAsync(string email, bool sucesso, string? detalhes = null, string? nomeUsuario = null, string? perfilUsuario = null)
        {
            try
            {
                var tipoAcao = sucesso ? "Login_Sucesso" : "Login_Falha";
                var nivelSeveridade = sucesso ? "Info" : "Warning";

                    string descricao;
                if (!string.IsNullOrEmpty(nomeUsuario))
                {
                    descricao = sucesso
                        ? $"Login realizado por {nomeUsuario}"
                        : $"Falha no login para {nomeUsuario}";
                }
                else
                {
                    descricao = sucesso ? "Login realizado com sucesso" : "Tentativa de login falhada";
                }

                if (!string.IsNullOrEmpty(nomeUsuario) && !string.IsNullOrEmpty(email))
                {
                    var log = new Log
                    {
                        DataHora = DateTime.Now,
                        UsuarioEmail = email,
                        UsuarioNome = nomeUsuario,
                        PerfilUsuario = perfilUsuario ?? "Colaborador",
                        TipoAcao = tipoAcao,
                        Categoria = "Autenticação",
                        Descricao = descricao,
                        NivelSeveridade = nivelSeveridade,
                        DetalhesAdicionais = detalhes,
                        Ativo = true
                    };

                    _context.Logs.Add(log);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    await RegistrarAcaoAsync(tipoAcao, descricao, "Autenticação",
                        nivelSeveridade: nivelSeveridade, detalhesAdicionais: detalhes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao registrar log de login: {ex.Message}");
            }
        }

        public async Task RegistrarLogoutAsync(string email)
        {
            await RegistrarAcaoAsync("Logout", "Usuário fez logout do sistema", "Autenticação");
        }

        public async Task RegistrarTentativaAcessoNegadoAsync(string recurso, string? detalhes = null)
        {
            var descricao = $"Tentativa de acesso negado ao recurso: {recurso}";
            await RegistrarAcaoAsync("Acesso_Negado", descricao, "Segurança",
                nivelSeveridade: "Warning", detalhesAdicionais: detalhes);
        }

        public async Task RegistrarExcecaoAsync(Exception ex, string contexto)
        {
            var descricao = $"Exceção no contexto: {contexto}";
            var detalhes = $"Tipo: {ex.GetType().Name}\nMensagem: {ex.Message}\nStack Trace: {ex.StackTrace}";

            await RegistrarAcaoAsync("Exceção", descricao, "Sistema",
                nivelSeveridade: "Error", detalhesAdicionais: detalhes);
        }

        public async Task<List<Log>> ObterLogsAsync(DateTime? dataInicio = null, DateTime? dataFim = null,
            string? tipoAcao = null, string? categoria = null, string? nivelSeveridade = null,
            int pagina = 1, int itensPorPagina = 50)
        {
            var query = _context.Logs.Where(l => l.Ativo);

            if (dataInicio.HasValue)
                query = query.Where(l => l.DataHora >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(l => l.DataHora <= dataFim.Value.AddDays(1).AddTicks(-1));

            if (!string.IsNullOrEmpty(tipoAcao))
                query = query.Where(l => l.TipoAcao == tipoAcao);

            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(l => l.Categoria == categoria);

            if (!string.IsNullOrEmpty(nivelSeveridade))
                query = query.Where(l => l.NivelSeveridade == nivelSeveridade);

            return await query
                .OrderByDescending(l => l.DataHora)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();
        }

        public async Task<int> ContarLogsAsync(DateTime? dataInicio = null, DateTime? dataFim = null,
            string? tipoAcao = null, string? categoria = null, string? nivelSeveridade = null)
        {
            var query = _context.Logs.Where(l => l.Ativo);

            if (dataInicio.HasValue)
                query = query.Where(l => l.DataHora >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(l => l.DataHora <= dataFim.Value.AddDays(1).AddTicks(-1));

            if (!string.IsNullOrEmpty(tipoAcao))
                query = query.Where(l => l.TipoAcao == tipoAcao);

            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(l => l.Categoria == categoria);

            if (!string.IsNullOrEmpty(nivelSeveridade))
                query = query.Where(l => l.NivelSeveridade == nivelSeveridade);

            return await query.CountAsync();
        }

        public async Task<Dictionary<string, int>> ObterEstatisticasLogsAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var query = _context.Logs.Where(l => l.Ativo);

            if (dataInicio.HasValue)
                query = query.Where(l => l.DataHora >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(l => l.DataHora <= dataFim.Value.AddDays(1).AddTicks(-1));

            var estatisticas = new Dictionary<string, int>();

            var porCategoria = await query
                .GroupBy(l => l.Categoria)
                .Select(g => new { Categoria = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in porCategoria)
                estatisticas[$"Categoria_{item.Categoria}"] = item.Count;

            var porSeveridade = await query
                .GroupBy(l => l.NivelSeveridade)
                .Select(g => new { Severidade = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in porSeveridade)
                estatisticas[$"Severidade_{item.Severidade}"] = item.Count;

            var porPerfil = await query
                .GroupBy(l => l.PerfilUsuario)
                .Select(g => new { Perfil = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var item in porPerfil)
                estatisticas[$"Perfil_{item.Perfil}"] = item.Count;

            estatisticas["Total"] = await query.CountAsync();

            return estatisticas;
        }

    }
}