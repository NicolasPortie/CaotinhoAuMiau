using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Services
{
    public class EmailService
    {
        private readonly ApplicationDbContext _contexto;
        private readonly ILogger<EmailService> _logger;

        public EmailService(ApplicationDbContext contexto, ILogger<EmailService> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        public async Task<(bool sucesso, string mensagem)> EnviarEmailAsync(string destinatario, string assunto, string conteudo, string tipoEmail = "Geral", int? usuarioId = null, int? adocaoId = null, int? formularioId = null)
        {
            try
            {
                var config = await ObterConfiguracaoAtivaAsync();
                if (config == null)
                {
                    var mensagemErro = "Nenhuma configuração de email ativa encontrada";
                    _logger.LogError(mensagemErro);
                    return (false, mensagemErro);
                }

                using var cliente = new SmtpClient(config.ServidorSmtp, config.Porta)
                {
                    Credentials = new NetworkCredential(config.EmailRemetente, config.Senha),
                    EnableSsl = config.UsarSsl
                };

                var email = new MailMessage(config.EmailRemetente, destinatario, assunto, conteudo)
                {
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                await cliente.SendMailAsync(email);
                _logger.LogInformation($"Email enviado com sucesso para {destinatario}. Tipo: {tipoEmail}");

                return (true, "Email enviado com sucesso");
            }
            catch (Exception ex)
            {
                var mensagemErro = $"Erro ao enviar email: {ex.Message}";
                _logger.LogError(ex, $"Erro ao enviar email para {destinatario}");

                return (false, mensagemErro);
            }
        }

        public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string conteudo)
        {
            var resultado = await EnviarEmailAsync(destinatario, assunto, conteudo, "Geral");
            return resultado.sucesso;
        }

        public async Task<bool> EnviarEmailAprovacaoAdocaoAsync(int adocaoId)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .Where(a => a.Id == adocaoId)
                    .FirstOrDefaultAsync();

                if (adocao == null)
                {
                    _logger.LogWarning($"Adoção {adocaoId} não encontrada para envio de email de aprovação");
                    return false;
                }

                var assunto = $"Parabéns! Sua adoção foi aprovada - {adocao.Pet.Nome}";
                var conteudo = $@"
                    <h3>Sua solicitação de adoção foi aprovada!</h3>
                    <p>Olá {adocao.Usuario.Nome},</p>
                    <p>Temos uma ótima notícia! Sua solicitação para adotar <strong>{adocao.Pet.Nome}</strong> foi aprovada.</p>
                    <p>Em breve você receberá mais instruções sobre os próximos passos.</p>
                    <p>Atenciosamente,<br>Equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(adocao.Usuario.Email, assunto, conteudo, "Aprovacao_Adocao", adocao.UsuarioId, adocaoId);
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de aprovação de adoção {adocaoId}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailReprovacaoAdocaoAsync(int adocaoId, string motivo)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .Where(a => a.Id == adocaoId)
                    .FirstOrDefaultAsync();

                if (adocao == null)
                {
                    _logger.LogWarning($"Adoção {adocaoId} não encontrada para envio de email de reprovação");
                    return false;
                }

                var assunto = $"Informações sobre sua solicitação de adoção - {adocao.Pet.Nome}";
                var conteudo = $@"
                    <h3>Sobre sua solicitação de adoção</h3>
                    <p>Olá {adocao.Usuario.Nome},</p>
                    <p>Agradecemos seu interesse em adotar <strong>{adocao.Pet.Nome}</strong>.</p>
                    <p>Após análise cuidadosa, infelizmente não foi possível prosseguir com sua solicitação no momento.</p>
                    <p><strong>Motivo:</strong> {motivo}</p>
                    <p>Encorajamos você a continuar acompanhando nossos pets disponíveis e fazer novas solicitações.</p>
                    <p>Atenciosamente,<br>Equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(adocao.Usuario.Email, assunto, conteudo, "Reprovacao_Adocao", adocao.UsuarioId, adocaoId);
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de reprovação de adoção {adocaoId}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailNovoFormularioAsync(int formularioId)
        {
            try
            {
                var formulario = await _contexto.FormulariosAdocao
                    .Include(f => f.Usuario)
                    .Include(f => f.Pet)
                    .Where(f => f.Id == formularioId)
                    .FirstOrDefaultAsync();

                if (formulario == null)
                {
                    _logger.LogWarning($"Formulário {formularioId} não encontrado para envio de email");
                    return false;
                }

                var assunto = $"Formulário de adoção recebido - {formulario.Pet.Nome}";
                var conteudo = $@"
                    <h3>Seu formulário foi recebido com sucesso!</h3>
                    <p>Olá {formulario.Usuario.Nome},</p>
                    <p>Recebemos seu formulário de interesse em adotar <strong>{formulario.Pet.Nome}</strong>.</p>
                    <p>Nossa equipe irá analisar suas informações e entrar em contato em breve.</p>
                    <p>Obrigado por escolher a adoção responsável!</p>
                    <p>Atenciosamente,<br>Equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(formulario.Usuario.Email, assunto, conteudo, "Novo_Formulario", formulario.UsuarioId, formularioId: formularioId);
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de novo formulário {formularioId}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailVerificacaoAsync(string email, string token)
        {
            try
            {
                var assunto = "Verificação de Email - CaotinhoAuMiau";
                var linkVerificacao = $"https://caotinhoaumiau.com/verificar-email?token={token}&email={email}";

                var conteudo = $@"
                    <h3>Verificação de Email</h3>
                    <p>Para completar seu cadastro, clique no link abaixo:</p>
                    <p><a href='{linkVerificacao}'>Verificar Email</a></p>
                    <p>Se você não solicitou este cadastro, ignore este email.</p>
                    <p>Atenciosamente,<br>Equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(email, assunto, conteudo, "Verificacao_Email");
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de verificação para {email}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailFormularioAprovadoAsync(Models.Usuario usuario, Pet pet, int formularioId)
        {
            try
            {
                var assunto = $"Formulário aprovado - {pet.Nome}";
                var conteudo = $@"
                    <h3>Parabéns! Seu formulário foi aprovado!</h3>
                    <p>Olá {usuario.Nome},</p>
                    <p>Temos uma ótima notícia! Seu formulário para adoção de <strong>{pet.Nome}</strong> foi aprovado.</p>
                    <p>Em breve nossa equipe entrará em contato para os próximos passos do processo de adoção.</p>
                    <p>Obrigado por escolher a adoção responsável!</p>
                    <p>Atenciosamente,<br>Equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(usuario.Email, assunto, conteudo, "Formulario_Aprovado", usuario.Id, formularioId: formularioId);
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de formulário aprovado para usuário {usuario.Id}, formulário {formularioId}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailPrazoVencidoAsync(Models.Usuario usuario, Pet pet, int adocaoId)
        {
            try
            {
                var assunto = $"Prazo vencido - {pet.Nome}";
                var conteudo = $@"
                    <h3>Prazo para busca vencido</h3>
                    <p>Olá {usuario.Nome},</p>
                    <p>Infelizmente o prazo para buscar <strong>{pet.Nome}</strong> venceu.</p>
                    <p>A adoção foi cancelada pois o prazo para retirada venceu.</p>
                    <p>Se houver algum problema ou deseja explicar a situação, entre em contato conosco.</p>
                    <p>Atenciosamente,<br>Equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(usuario.Email, assunto, conteudo, "Prazo_Vencido", usuario.Id, adocaoId: adocaoId);
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de prazo vencido para usuário {usuario.Id}, adoção {adocaoId}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailLembretePrazoAsync(Models.Usuario usuario, Pet pet, int diasRestantes, DateTime dataLimite, int adocaoId)
        {
            try
            {
                var assunto = $"Lembrete: {diasRestantes} dias para buscar {pet.Nome}";
                var conteudo = $@"
                    <h3>Lembrete importante!</h3>
                    <p>Olá {usuario.Nome},</p>
                    <p>Este é um lembrete de que você tem apenas <strong>{diasRestantes} dia(s)</strong> para buscar <strong>{pet.Nome}</strong>.</p>
                    <p>Data limite: <strong>{dataLimite:dd/MM/yyyy}</strong></p>
                    <p>Entre em contato conosco para agendar a busca ou se tiver alguma dificuldade.</p>
                    <p>Não perca essa oportunidade!</p>
                    <p>Atenciosamente,<br>Equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(usuario.Email, assunto, conteudo, "Lembrete_Prazo", usuario.Id, adocaoId: adocaoId);
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de lembrete para usuário {usuario.Id}, adoção {adocaoId}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailFinalizacaoAdocaoAsync(int adocaoId)
        {
            try
            {
                var adocao = await _contexto.Adocoes
                    .Include(a => a.Usuario)
                    .Include(a => a.Pet)
                    .Where(a => a.Id == adocaoId)
                    .FirstOrDefaultAsync();

                if (adocao == null)
                {
                    _logger.LogWarning($"Adoção {adocaoId} não encontrada para envio de email de finalização");
                    return false;
                }

                var assunto = $"Parabéns! Adoção de {adocao.Pet.Nome} finalizada com sucesso!";
                var conteudo = $@"
                    <h3>Adoção finalizada com sucesso!</h3>
                    <p>Olá {adocao.Usuario.Nome},</p>
                    <p>É com muita alegria que confirmamos que a adoção de <strong>{adocao.Pet.Nome}</strong> foi finalizada com sucesso!</p>
                    <p>Agradecemos imensamente por escolher a adoção responsável e por dar uma nova chance de amor a um pet que precisava de um lar.</p>

                    <h4>Próximos passos importantes:</h4>
                    <ul>
                        <li>Mantenha a vacinação e cuidados veterinários em dia</li>
                        <li>Proporcione muito amor, carinho e atenção</li>
                        <li>Se precisar de dicas ou apoio, não hesite em nos contatar</li>
                        <li>Compartilhe fotos e momentos felizes conosco!</li>
                    </ul>

                    <p>Desejamos muito amor, alegria e momentos inesquecíveis com seu novo companheiro!</p>
                    <p>Muito obrigado por fazer a diferença na vida de um animal!</p>

                    <p>Com carinho,<br>Toda a equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(adocao.Usuario.Email, assunto, conteudo, "Finalizacao_Adocao", adocao.UsuarioId, adocaoId);
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de finalização de adoção {adocaoId}");
                return false;
            }
        }

        public async Task<bool> EnviarEmailCancelamentoAdocaoAsync(int usuarioId, int petId, string motivo)
        {
            try
            {
                var usuario = await _contexto.Usuarios.FindAsync(usuarioId);
                var pet = await _contexto.Pets.FindAsync(petId);

                if (usuario == null || pet == null)
                {
                    _logger.LogWarning($"Usuário {usuarioId} ou Pet {petId} não encontrado para envio de email de cancelamento");
                    return false;
                }

                var assunto = $"Cancelamento da adoção de {pet.Nome}";
                var conteudo = $@"
                    <h3>Adoção cancelada</h3>
                    <p>Olá {usuario.Nome},</p>
                    <p>Lamentamos informar que sua adoção de <strong>{pet.Nome}</strong> foi cancelada.</p>

                    <h4>Motivo do cancelamento:</h4>
                    <div style='background-color: #f8d7da; padding: 15px; border-left: 4px solid #dc3545; margin: 15px 0;'>
                        <p style='margin: 0; color: #721c24;'>{motivo}</p>
                    </div>

                    <p>Entendemos que esta notícia pode ser decepcionante. Se você tiver dúvidas sobre esta decisão, entre em contato conosco.</p>
                    <p>Não desista! Temos outros pets esperando por uma família carinhosa como a sua. Continue explorando nossos pets disponíveis.</p>

                    <p>Atenciosamente,<br>Equipe CaotinhoAuMiau</p>";

                var resultado = await EnviarEmailAsync(usuario.Email, assunto, conteudo, "Cancelamento_Adocao", usuarioId);
                return resultado.sucesso;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar email de cancelamento para usuário {usuarioId} e pet {petId}");
                return false;
            }
        }

        private async Task<ConfiguracaoEmail?> ObterConfiguracaoAtivaAsync()
        {
            return await _contexto.ConfiguracoesEmail
                .Where(c => c.Ativo)
                .OrderByDescending(c => c.DataCriacao)
                .FirstOrDefaultAsync();
        }
    }
}