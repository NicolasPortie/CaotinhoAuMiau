using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.Enums;
using Microsoft.Extensions.Logging;

namespace CaotinhoAuMiau.Services
{
    public class AdocaoAutomacaoService
    {
        private readonly ApplicationDbContext _contexto;
        private readonly EmailService _emailService;
        private readonly NotificationService _notificationService;
        private readonly ILogger<AdocaoAutomacaoService> _logger;

        public AdocaoAutomacaoService(ApplicationDbContext contexto, EmailService emailService, NotificationService notificationService, ILogger<AdocaoAutomacaoService> logger)
        {
            _contexto = contexto;
            _emailService = emailService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task ProcessarCancelamentosAutomaticosAsync()
        {
            try
            {
                await CancelarContratosNaoAssinadosAsync();

                await CancelarAdocoesPorPrazoVencidoAsync();

                _logger.LogInformation("Verificação de prazos vencidos concluída");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante processamento de cancelamentos automáticos");
            }
        }

        private async Task CancelarContratosNaoAssinadosAsync()
        {
            var dataLimiteAssinatura = DateTime.Now.AddDays(-3);

            var adocoesVencidas = await _contexto.Adocoes
                .Include(a => a.Pet)
                .Include(a => a.Usuario)
                .Where(a => a.Status == StatusAdocao.AguardandoAssinarContrato &&
                           a.DataResposta.HasValue && 
                           a.DataResposta.Value <= dataLimiteAssinatura)
                .ToListAsync();

            foreach (var adocao in adocoesVencidas)
            {
                adocao.Status = StatusAdocao.CanceladoPorNaoAssinarContrato;

                if (adocao.Pet != null)
                {
                    adocao.Pet.Status = StatusPet.Disponivel;
                    _contexto.Pets.Update(adocao.Pet);
                }

                if (adocao.Usuario != null)
                {
                    await _emailService.EnviarEmailPrazoVencidoAsync(adocao.Usuario, adocao.Pet, adocao.Id);

                    await _notificationService.CriarNotificacaoAsync(
                        adocao.Usuario.Id.ToString(),
                        "Adoção Cancelada - Contrato",
                        $"Sua adoção do {adocao.Pet.Nome} foi cancelada por não assinar o contrato no prazo.",
                        "cancelamento",
                        $"adocao_{adocao.Id}"
                    );
                }

                _contexto.Adocoes.Update(adocao);
                
                _logger.LogInformation("Adoção {AdocaoId} cancelada por prazo vencido - contrato não foi assinado", adocao.Id);
            }

            if (adocoesVencidas.Any())
            {
                await _contexto.SaveChangesAsync();
                _logger.LogInformation("Cancelados {Count} contratos não assinados", adocoesVencidas.Count);
            }
        }

        private async Task CancelarAdocoesPorPrazoVencidoAsync()
        {
            var adocoesVencidas = await _contexto.Adocoes
                .Include(a => a.Pet)
                .Include(a => a.Usuario)
                .Include(a => a.Contrato)
                .Where(a => a.Status == StatusAdocao.AguardandoBuscar &&
                           a.Contrato != null &&
                           a.Contrato.DataAssinatura.HasValue)
                .ToListAsync();

            adocoesVencidas = adocoesVencidas.Where(a =>
            {
                var dataLimite = CalcularDataLimiteRetirada(a.Contrato.DataAssinatura.Value);
                return DateTime.Now.Date > dataLimite.Date;
            }).ToList();

            foreach (var adocao in adocoesVencidas)
            {
                adocao.Status = StatusAdocao.CanceladoPorPrazoVencido;

                if (adocao.Pet != null)
                {
                    adocao.Pet.Status = StatusPet.Disponivel;
                    _contexto.Pets.Update(adocao.Pet);
                }

                if (adocao.Usuario != null)
                {
                    await _emailService.EnviarEmailPrazoVencidoAsync(adocao.Usuario, adocao.Pet, adocao.Id);

                    await _notificationService.CriarNotificacaoAsync(
                        adocao.Usuario.Id.ToString(),
                        "Adoção Cancelada - Prazo Vencido",
                        $"Sua adoção do {adocao.Pet.Nome} foi cancelada por não buscar o pet no prazo de 5 dias úteis.",
                        "cancelamento",
                        $"adocao_{adocao.Id}"
                    );
                }

                _contexto.Adocoes.Update(adocao);

                _logger.LogInformation("Adoção {AdocaoId} cancelada por prazo vencido - pet não foi retirado", adocao.Id);
            }

            if (adocoesVencidas.Any())
            {
                await _contexto.SaveChangesAsync();
                _logger.LogInformation("Canceladas {Count} adoções por não buscar no prazo", adocoesVencidas.Count);
            }
        }

        public async Task EnviarLembretesPrazoAsync()
        {
            var adocoesParaLembrete = await _contexto.Adocoes
                .Include(a => a.Pet)
                .Include(a => a.Usuario)
                .Include(a => a.Contrato)
                .Where(a => a.Status == StatusAdocao.AguardandoBuscar &&
                           a.Contrato != null &&
                           a.Contrato.DataAssinatura.HasValue)
                .ToListAsync();

            var adocoesPara2Dias = adocoesParaLembrete.Where(a =>
            {
                var dataLimite = CalcularDataLimiteRetirada(a.Contrato.DataAssinatura.Value);
                var diasRestantes = (dataLimite.Date - DateTime.Now.Date).Days;
                return diasRestantes == 2;
            }).ToList();

            foreach (var adocao in adocoesPara2Dias)
            {
                if (adocao.Usuario != null && adocao.Pet != null && adocao.Contrato?.DataAssinatura != null)
                {
                    var dataLimite = CalcularDataLimiteRetirada(adocao.Contrato.DataAssinatura.Value);
                    var diasRestantes = (dataLimite.Date - DateTime.Now.Date).Days;

                    await _emailService.EnviarEmailLembretePrazoAsync(adocao.Usuario, adocao.Pet, diasRestantes, dataLimite, adocao.Id);

                    await _notificationService.CriarNotificacaoAsync(
                        adocao.Usuario.Id.ToString(),
                        "Lembrete: Buscar Pet",
                        $"Faltam apenas {diasRestantes} dia(s) para buscar o {adocao.Pet.Nome}! Prazo até {dataLimite:dd/MM/yyyy}.",
                        "lembrete",
                        $"adocao_{adocao.Id}"
                    );

                    _logger.LogInformation("Lembrete de prazo enviado para adoção {AdocaoId} - {DiasRestantes} dias restantes", adocao.Id, diasRestantes);
                }
            }
        }

        private static DateTime CalcularDataLimiteRetirada(DateTime dataAssinatura)
        {
            var dataLimite = dataAssinatura.Date;
            var diasAdicionados = 0;

            while (diasAdicionados < 5)
            {
                dataLimite = dataLimite.AddDays(1);

                if (dataLimite.DayOfWeek != DayOfWeek.Saturday &&
                    dataLimite.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasAdicionados++;
                }
            }

            return dataLimite;
        }
    }
}