using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CaotinhoAuMiau.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(IServiceScopeFactory scopeFactory, ILogger<EmailBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Service iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var automacaoService = scope.ServiceProvider.GetRequiredService<AdocaoAutomacaoService>();

                    await automacaoService.ProcessarCancelamentosAutomaticosAsync();
                    await automacaoService.EnviarLembretesPrazoAsync();

                    _logger.LogInformation("Processamento de automação de adoções concluído");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante processamento de emails automáticos");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }

            _logger.LogInformation("Email Background Service finalizado");
        }
    }
}