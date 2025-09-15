using CaotinhoAuMiau.Services;

namespace CaotinhoAuMiau.Utils
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuditoriaService _auditoriaService;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, IAuditoriaService auditoriaService, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _auditoriaService = auditoriaService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                var method = context.Request.Method;
                var path = context.Request.Path;
                var queryString = context.Request.QueryString.ToString();

                if (ShouldLogRequest(path, method))
                {
                    var requestDetails = $"Método: {method}, Caminho: {path}{queryString}";
                    await _auditoriaService.RegistrarAcaoAsync(
                        "Requisição_HTTP",
                        $"Requisição {method} para {path}",
                        "Sistema",
                        detalhesAdicionais: requestDetails
                    );
                }

                await _next(context);

                stopwatch.Stop();

                if (ShouldLogRequest(path, method) && context.Response.StatusCode >= 400)
                {
                    var responseDetails = $"Status: {context.Response.StatusCode}, Tempo: {stopwatch.ElapsedMilliseconds}ms";
                    var severidade = context.Response.StatusCode >= 500 ? "Error" : "Warning";

                    await _auditoriaService.RegistrarAcaoAsync(
                        "Resposta_HTTP_Erro",
                        $"Erro na requisição {method} {path} - Status {context.Response.StatusCode}",
                        "Sistema",
                        nivelSeveridade: severidade,
                        detalhesAdicionais: responseDetails
                    );
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _auditoriaService.RegistrarExcecaoAsync(ex, $"Middleware - {context.Request.Method} {context.Request.Path}");

                _logger.LogError(ex, "Erro não tratado na requisição {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                throw;
            }
        }

        private static bool ShouldLogRequest(string path, string method)
        {
            if (path.StartsWith("/css/") || path.StartsWith("/js/") ||
                path.StartsWith("/images/") || path.StartsWith("/lib/") ||
                path.StartsWith("/imagens/") || path.Contains(".css") ||
                path.Contains(".js") || path.Contains(".png") ||
                path.Contains(".jpg") || path.Contains(".jpeg") ||
                path.Contains(".gif") || path.Contains(".svg") ||
                path.Contains(".ico"))
            {
                return false;
            }

            return (IsAdministrativeRequest(path) || IsImportantUserRequest(path)) &&
                   (method == "POST" || method == "PUT" || method == "DELETE" ||
                    (method == "GET" && IsImportantGetRequest(path)));
        }

        private static bool IsAdministrativeRequest(string path)
        {
            var adminPaths = new[]
            {
                "/Admin/",
                "/admin/",
                "/api/admin/",
                "/autenticacao/" // Apenas login/logout de colaboradores
            };

            return adminPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsImportantUserRequest(string path)
        {
            var userPaths = new[]
            {
                "/usuario/adocao/formulario/",
                "/usuario/contrato/",
                "/usuario/perfil/",
                "/usuario/adocoes/",
                "/api/notificacao/"
            };

            return userPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsImportantGetRequest(string path)
        {
            var importantPaths = new[]
            {
                "/Admin/",
                "/admin/",
                "/api/admin/",
                "/autenticacao/",
                "/usuario/adocao/formulario/",
                "/usuario/contrato/",
                "/usuario/perfil/",
                "/usuario/adocoes/"
            };

            return importantPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
    }
}