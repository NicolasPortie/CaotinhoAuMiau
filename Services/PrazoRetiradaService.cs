using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CaotinhoAuMiau.Services
{
    public interface IPrazoRetiradaService
    {
        Task<(bool podeAdotar, string motivoBloqueio, DateTime? fimQuarentena)> VerificarQuarentenaUsuarioAsync(int usuarioId, int petId);
        Task<bool> UsuarioEstaEmQuarentenaAsync(int usuarioId, int petId);
        Task IniciarQuarentenaAsync(int usuarioId, int petId, string motivo);
        Task<DateTime?> ObterFimQuarentenaAsync(int usuarioId, int? petId = null);
    }

    public class PrazoRetiradaService : IPrazoRetiradaService
    {
        private readonly ApplicationDbContext _contexto;

        private const int QUARENTENA_CANCELAMENTO_USUARIO = 15;
        private const int QUARENTENA_REJEICAO_ADMIN = 30;
        private const int QUARENTENA_INATIVIDADE = 7;
        private const int QUARENTENA_MULTIPLOS_CANCELAMENTOS = 60;

        public PrazoRetiradaService(ApplicationDbContext contexto)
        {
            _contexto = contexto;
        }

        public async Task<(bool podeAdotar, string motivoBloqueio, DateTime? fimQuarentena)> VerificarQuarentenaUsuarioAsync(int usuarioId, int petId)
        {
            var historicoPet = await _contexto.FormulariosAdocao
                .Where(f => f.UsuarioId == usuarioId && f.PetId == petId)
                .OrderByDescending(f => f.DataEnvio)
                .FirstOrDefaultAsync();

            if (historicoPet != null)
            {
                var diasDecorridos = (DateTime.Now - historicoPet.DataEnvio).Days;
                var diasQuarentena = ObterDiasQuarentena(historicoPet.Status, historicoPet.ObservacoesCancelamento);

                if (diasDecorridos < diasQuarentena)
                {
                    var fimQuarentena = historicoPet.DataEnvio.AddDays(diasQuarentena);
                    var motivo = ObterMotivoQuarentena(historicoPet.Status);
                    return (false, motivo, fimQuarentena);
                }
            }

            var seisMesesAtras = DateTime.Now.AddMonths(-6);
            var cancelamentosRecentes = await _contexto.FormulariosAdocao
                .Where(f => f.UsuarioId == usuarioId &&
                           f.DataEnvio >= seisMesesAtras &&
                           (f.StatusEnum == StatusFormulario.CanceladoPeloUsuario || f.StatusEnum == StatusFormulario.CanceladoPorInatividade))
                .CountAsync();

            if (cancelamentosRecentes >= 3)
            {
                var ultimoCancelamento = await _contexto.FormulariosAdocao
                    .Where(f => f.UsuarioId == usuarioId &&
                               (f.StatusEnum == StatusFormulario.CanceladoPeloUsuario || f.StatusEnum == StatusFormulario.CanceladoPorInatividade))
                    .OrderByDescending(f => f.DataEnvio)
                    .FirstOrDefaultAsync();

                if (ultimoCancelamento != null)
                {
                    var diasDecorridos = (DateTime.Now - ultimoCancelamento.DataEnvio).Days;
                    if (diasDecorridos < QUARENTENA_MULTIPLOS_CANCELAMENTOS)
                    {
                        var fimQuarentena = ultimoCancelamento.DataEnvio.AddDays(QUARENTENA_MULTIPLOS_CANCELAMENTOS);
                        return (false, "Muitos cancelamentos recentes. Quarentena estendida aplicada.", fimQuarentena);
                    }
                }
            }

            return (true, string.Empty, null);
        }

        public async Task<bool> UsuarioEstaEmQuarentenaAsync(int usuarioId, int petId)
        {
            var (podeAdotar, _, _) = await VerificarQuarentenaUsuarioAsync(usuarioId, petId);
            return !podeAdotar;
        }

        public async Task IniciarQuarentenaAsync(int usuarioId, int petId, string motivo)
        {
            var formulario = await _contexto.FormulariosAdocao
                .Where(f => f.UsuarioId == usuarioId && f.PetId == petId)
                .OrderByDescending(f => f.DataEnvio)
                .FirstOrDefaultAsync();

            if (formulario != null)
            {
                formulario.ObservacoesCancelamento = $"Quarentena iniciada: {motivo}";
                await _contexto.SaveChangesAsync();
            }
        }

        public async Task<DateTime?> ObterFimQuarentenaAsync(int usuarioId, int? petId = null)
        {
            var query = _contexto.FormulariosAdocao
                .Where(f => f.UsuarioId == usuarioId &&
                           (f.StatusEnum == StatusFormulario.CanceladoPeloUsuario || f.StatusEnum == StatusFormulario.CanceladoPorInatividade ||
                            f.StatusEnum == StatusFormulario.Negado));

            if (petId.HasValue)
            {
                query = query.Where(f => f.PetId == petId.Value);
            }

            var ultimoCancelamento = await query
                .OrderByDescending(f => f.DataEnvio)
                .FirstOrDefaultAsync();

            if (ultimoCancelamento != null)
            {
                var diasQuarentena = ObterDiasQuarentena(ultimoCancelamento.Status, ultimoCancelamento.ObservacoesCancelamento);
                var fimQuarentena = ultimoCancelamento.DataEnvio.AddDays(diasQuarentena);

                if (DateTime.Now < fimQuarentena)
                {
                    return fimQuarentena;
                }
            }

            return null;
        }

        private int ObterDiasQuarentena(string status, string? observacoes)
        {
            return status switch
            {
                "CanceladoPeloUsuario" or "Cancelado" => QUARENTENA_CANCELAMENTO_USUARIO,
                "Rejeitado" => QUARENTENA_REJEICAO_ADMIN,
                "CanceladoPorInatividade" => QUARENTENA_INATIVIDADE,
                _ when observacoes?.Contains("Múltiplos cancelamentos") == true => QUARENTENA_MULTIPLOS_CANCELAMENTOS,
                _ => QUARENTENA_CANCELAMENTO_USUARIO
            };
        }

        private string ObterMotivoQuarentena(string status)
        {
            return status switch
            {
                "CanceladoPeloUsuario" or "Cancelado" => "Aguarde o período de quarentena após cancelamento.",
                "Rejeitado" => "Formulário foi rejeitado. Aguarde o período de quarentena.",
                "CanceladoPorInatividade" => "Formulário cancelado por inatividade. Aguarde o período de quarentena.",
                _ => "Aguarde o período de quarentena."
            };
        }
    }
}