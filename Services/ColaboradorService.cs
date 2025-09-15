using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CaotinhoAuMiau.Services
{
    public interface IColaboradorService
    {
        Task<Colaborador?> AutenticarAsync(string email, string senha);
        Task CriarAdminPadraoAsync();
    }

    public class ColaboradorService : IColaboradorService
    {
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<ColaboradorService> _logger;
        private readonly AdminConfig _adminConfig;

        public ColaboradorService(ApplicationDbContext contexto, ILogger<ColaboradorService> logger, IOptions<AdminConfig> adminConfig)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
            _logger = logger;
            _adminConfig = adminConfig.Value;
        }

        public async Task<Colaborador?> AutenticarAsync(string email, string senha)
        {
            var colaborador = await _contexto.Colaboradores
                .FirstOrDefaultAsync(c => c.Email == email && c.Ativo);

            if (colaborador != null && HashHelper.VerificarSenha(senha, colaborador.Senha))
                return colaborador;

            return null;
        }

        public async Task CriarAdminPadraoAsync()
        {
            try
            {
                var adminAntigo = await _contexto.Colaboradores
                    .FirstOrDefaultAsync(c => c.Email == _adminConfig.LegacyAdminEmail);
                
                if (adminAntigo != null)
                {
                    _contexto.Colaboradores.Remove(adminAntigo);
                    await _contexto.SaveChangesAsync();
                }

                var colaboradorAdmin = await _contexto.Colaboradores
                    .FirstOrDefaultAsync(c => c.Email == _adminConfig.DefaultAdminEmail || c.CPF == "00000000000");

                if (colaboradorAdmin == null)
                {
                    colaboradorAdmin = new Colaborador
                    {
                        UsuarioId = null,
                        Nome = "Administrador Sistema",
                        Email = _adminConfig.DefaultAdminEmail,
                        CPF = "00000000000",
                        Telefone = "0000000000",
                        Cargo = CargoColaboradorEnum.Administrador,
                        Senha = HashHelper.GerarHashSenha("admin"),
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };

                    _contexto.Colaboradores.Add(colaboradorAdmin);
                    await _contexto.SaveChangesAsync();
                    _logger.LogInformation("Admin padrão criado com sucesso.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar admin padrão");
                throw;
            }
        }
    }
}