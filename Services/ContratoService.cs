using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using System;
using System.Threading.Tasks;

namespace CaotinhoAuMiau.Services
{
    public class ContratoService
    {
        private readonly ApplicationDbContext _contexto;
        private readonly NotificationService _servicoNotificacao;

        public ContratoService(ApplicationDbContext contexto, NotificationService servicoNotificacao)
        {
            _contexto = contexto;
            _servicoNotificacao = servicoNotificacao;
        }

        public async Task<(bool sucesso, string mensagem, ContratoAdocao? contrato)> GerarContratoAsync(int adocaoId)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Pet)
                    .Include(a => a.Usuario)
                    .FirstOrDefaultAsync(a => a.Id == adocaoId);

                if (adocao == null)
                {
                    return (false, "Adoção não encontrada.", null);
                }

                if (adocao.Status != Models.Enums.StatusAdocao.AguardandoAssinarContrato)
                {
                    return (false, "Contrato só pode ser gerado para adoções aprovadas.", null);
                }

                var contratoExistente = await _contexto.ContratosAdocao
                    .FirstOrDefaultAsync(c => c.AdocaoId == adocaoId);

                if (contratoExistente != null)
                {
                    return (true, "Contrato já existe.", contratoExistente);
                }

                var conteudoContrato = GerarConteudoContrato(adocao);

                var contrato = new ContratoAdocao
                {
                    AdocaoId = adocaoId,
                    ConteudoContrato = conteudoContrato,
                    StatusContrato = "Pendente",
                    DataCriacao = DateTime.Now
                };

                _contexto.ContratosAdocao.Add(contrato);

                adocao.Status = Models.Enums.StatusAdocao.AguardandoAssinarContrato;
                adocao.ContratoId = contrato.Id;

                await _contexto.SaveChangesAsync();

                await _servicoNotificacao.CriarNotificacaoAsync(
                    adocao.UsuarioId.ToString(),
                    "Contrato disponível para assinatura",
                    $"O contrato de adoção do pet {adocao.Pet?.Nome} está disponível para assinatura. Clique aqui para assinar.",
                    "Contrato",
                    adocao.Id.ToString()
                );

                return (true, "Contrato gerado com sucesso!", contrato);
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao gerar contrato: {ex.Message}", null);
            }
        }

        public async Task<(bool sucesso, string mensagem)> AssinarContratoAsync(int contratoId, string assinaturaUsuario, int usuarioId)
        {
            try
            {
                var contrato = await _contexto.ContratosAdocao
                    .Include(c => c.Adocao)
                    .ThenInclude(a => a.Pet)
                    .FirstOrDefaultAsync(c => c.Id == contratoId);

                if (contrato == null)
                {
                    return (false, "Contrato não encontrado.");
                }

                if (contrato.Adocao?.UsuarioId != usuarioId)
                {
                    return (false, "Usuário não autorizado para assinar este contrato.");
                }

                if (contrato.StatusContrato == "Assinado")
                {
                    return (false, "Contrato já foi assinado.");
                }

                if (contrato.EstaExpirado)
                {
                    return (false, "Contrato expirado. Solicite um novo contrato.");
                }

                if (string.IsNullOrWhiteSpace(assinaturaUsuario))
                {
                    return (false, "Assinatura é obrigatória.");
                }

                contrato.AssinaturaUsuario = assinaturaUsuario;
                contrato.DataAssinatura = DateTime.Now;
                contrato.StatusContrato = "Assinado";
                
                contrato.ConteudoContrato = AdicionarAssinaturaAoContrato(contrato.ConteudoContrato, assinaturaUsuario, contrato.DataAssinatura.Value);

                if (contrato.Adocao != null)
                {
                    contrato.Adocao.ContratoAssinado = true;
                    contrato.Adocao.Status = Models.Enums.StatusAdocao.AguardandoBuscar;
                }

                await _contexto.SaveChangesAsync();

                await _servicoNotificacao.CriarNotificacaoAsync(
                    usuarioId.ToString(),
                    "Contrato assinado com sucesso",
                    $"O contrato de adoção do pet {contrato.Adocao?.Pet?.Nome} foi assinado. Agora você pode agendar a retirada.",
                    "Contrato",
                    contrato.AdocaoId.ToString()
                );

                return (true, "Contrato assinado com sucesso!");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao assinar contrato: {ex.Message}");
            }
        }

        public async Task<ContratoAdocao?> ObterContratoPorAdocaoAsync(int adocaoId, int usuarioId)
        {
            return await _contexto.ContratosAdocao
                .Include(c => c.Adocao)
                .ThenInclude(a => a.Pet)
                .Include(c => c.Adocao)
                .ThenInclude(a => a.Usuario)
                .FirstOrDefaultAsync(c => c.AdocaoId == adocaoId && c.Adocao!.UsuarioId == usuarioId);
        }

        public async Task<(bool sucesso, string mensagem)> VerificarStatusContratoAsync(int adocaoId)
        {
            try
            {
                var contrato = await _contexto.ContratosAdocao
                    .FirstOrDefaultAsync(c => c.AdocaoId == adocaoId);

                if (contrato == null)
                {
                    return (false, "Contrato não encontrado.");
                }

                if (contrato.EstaExpirado)
                {
                    var adocao = await _contexto.Adocoes.FindAsync(adocaoId);
                    if (adocao != null)
                    {
                        adocao.Status = Models.Enums.StatusAdocao.AguardandoAssinarContrato;
                    }

                    _contexto.ContratosAdocao.Remove(contrato);
                    await _contexto.SaveChangesAsync();

                    return (false, "Contrato expirado e removido. Um novo contrato pode ser gerado.");
                }

                return (true, contrato.StatusContrato);
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao verificar status: {ex.Message}");
            }
        }

        private string GerarConteudoContrato(Adocao adocao)
        {
            var dataAtual = DateTime.Now.ToString("dd/MM/yyyy");
            
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 800px; margin: 0 auto; padding: 20px;'>
                    <div style='text-align: center; margin-bottom: 30px;'>
                        <h1 style='color: #E67E22; margin-bottom: 10px;'>CONTRATO DE ADOÇÃO DE ANIMAL</h1>
                        <h2 style='color: #D35400; margin: 0;'>CaotinhoAuMiau</h2>
                    </div>

                    <div style='margin-bottom: 20px;'>
                        <h3 style='color: #E67E22; border-bottom: 2px solid #E67E22; padding-bottom: 5px;'>DADOS DO PET</h3>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; font-weight: bold; width: 30%;'>Nome:</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>{adocao.Pet?.Nome}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; font-weight: bold;'>Espécie:</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>{adocao.Pet?.Especie}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; font-weight: bold;'>Raça:</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>{adocao.Pet?.Raca}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; font-weight: bold;'>Idade:</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>{adocao.Pet?.Anos} anos e {adocao.Pet?.Meses} meses</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; font-weight: bold;'>Sexo:</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>{adocao.Pet?.Sexo}</td>
                            </tr>
                        </table>
                    </div>

                    <div style='margin-bottom: 20px;'>
                        <h3 style='color: #E67E22; border-bottom: 2px solid #E67E22; padding-bottom: 5px;'>DADOS DO ADOTANTE</h3>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; font-weight: bold; width: 30%;'>Nome:</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>{adocao.Usuario?.Nome}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; font-weight: bold;'>Email:</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>{adocao.Usuario?.Email}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; font-weight: bold;'>Telefone:</td>
                                <td style='padding: 8px; border: 1px solid #ddd;'>{adocao.Usuario?.Telefone}</td>
                            </tr>
                        </table>
                    </div>

                    <div style='margin-bottom: 20px;'>
                        <h3 style='color: #E67E22; border-bottom: 2px solid #E67E22; padding-bottom: 5px;'>TERMOS E CONDIÇÕES</h3>
                        <div style='text-align: justify; line-height: 1.6;'>
                            <p><strong>1. RESPONSABILIDADES DO ADOTANTE:</strong></p>
                            <ul style='margin-left: 20px;'>
                                <li>Fornecer alimentação adequada, água fresca e abrigo ao animal;</li>
                                <li>Providenciar cuidados veterinários necessários, incluindo vacinação e vermifugação;</li>
                                <li>Manter o animal em ambiente seguro e adequado;</li>
                                <li>Não abandonar, maltratar ou ceder o animal a terceiros sem autorização;</li>
                                <li>Permitir visitas da equipe CaotinhoAuMiau para acompanhamento, se necessário.</li>
                            </ul>

                            <p><strong>2. COMPROMISSOS:</strong></p>
                            <ul style='margin-left: 20px;'>
                                <li>O adotante se compromete a cuidar do animal com amor e responsabilidade;</li>
                                <li>Em caso de impossibilidade de manter o animal, o adotante deve entrar em contato com o CaotinhoAuMiau;</li>
                                <li>O animal não poderá ser comercializado ou utilizado para reprodução sem autorização;</li>
                                <li>Castração é altamente recomendada e pode ser condição para adoção.</li>
                            </ul>

                            <p><strong>3. RESCISÃO:</strong></p>
                            <p>Este contrato pode ser rescindido em caso de descumprimento das condições estabelecidas, com a devolução do animal ao CaotinhoAuMiau.</p>
                        </div>
                    </div>

                    <div style='margin-top: 30px; text-align: center;'>
                        <p>Data: {dataAtual}</p>
                        <p style='margin-top: 40px;'>
                            <strong>Declaro que li e concordo com todos os termos deste contrato.</strong>
                        </p>
                    </div>
                </div>";
        }

        private string AdicionarAssinaturaAoContrato(string conteudoOriginal, string assinaturaBase64, DateTime dataAssinatura)
        {
            var secaoAssinatura = $@"
                    <div style='margin-top: 40px; border-top: 2px solid #E67E22; padding-top: 20px;'>
                        <h3 style='color: #E67E22; border-bottom: 2px solid #E67E22; padding-bottom: 5px;'>ASSINATURA DIGITAL</h3>
                        <div style='display: flex; align-items: center; gap: 20px; background-color: #f9f9f9; padding: 15px; border-radius: 8px; border: 1px solid #ddd;'>
                            <div style='flex: 1;'>
                                <p style='margin: 0 0 5px 0; font-weight: bold;'>Assinado digitalmente em:</p>
                                <p style='margin: 0 0 10px 0; color: #666;'>{dataAssinatura:dd/MM/yyyy} às {dataAssinatura:HH:mm:ss}</p>
                                <p style='margin: 0; font-size: 12px; color: #888;'>Assinatura verificada e autenticada pelo sistema CaotinhoAuMiau</p>
                            </div>
                            <div style='border-left: 2px solid #E67E22; padding-left: 20px;'>
                                <img src='{assinaturaBase64}' alt='Assinatura digital' style='max-width: 200px; max-height: 80px; border: 1px solid #ddd; border-radius: 4px;' />
                            </div>
                        </div>
                    </div>";

            var ultimoIndex = conteudoOriginal.LastIndexOf("</div>");
            if (ultimoIndex > 0)
            {
                return conteudoOriginal.Substring(0, ultimoIndex) + secaoAssinatura + "\n                </div>";
            }
            return conteudoOriginal + secaoAssinatura;
        }
    }
}