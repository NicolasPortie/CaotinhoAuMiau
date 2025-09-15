using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CaotinhoAuMiau.Utils
{
    public static class ErrorHandler
    {
        public static void LogError<T>(ILogger<T> logger, Exception ex, string mensagem, params object[] parametros)
        {
            logger.LogError(ex, mensagem, parametros);
        }

        public static void LogWarning<T>(ILogger<T> logger, Exception ex, string mensagem, params object[] parametros)
        {
            logger.LogWarning(ex, mensagem, parametros);
        }

        public static string GerarMensagemErroUsuario(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => "Você não tem permissão para realizar esta operação.",
                ArgumentException => "Dados inválidos fornecidos.",
                InvalidOperationException => "Operação não pode ser realizada no momento.",
                TimeoutException => "A operação demorou muito tempo. Tente novamente.",
                _ => "Ocorreu um erro interno. Tente novamente ou contate o suporte."
            };
        }

        public static object GerarRespostaErro(Exception ex, bool incluirDetalhes = false)
        {
            var resposta = new
            {
                sucesso = false,
                mensagem = GerarMensagemErroUsuario(ex),
                timestamp = DateTime.UtcNow
            };

            if (incluirDetalhes)
            {
                return new
                {
                    sucesso = false,
                    mensagem = GerarMensagemErroUsuario(ex),
                    detalhes = ex.Message,
                    tipo = ex.GetType().Name,
                    timestamp = DateTime.UtcNow
                };
            }

            return resposta;
        }

        public static T ExecutarComTratamento<T>(Func<T> operacao, ILogger logger, string contexto = "Operação")
        {
            try
            {
                return operacao();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro durante {Contexto}", contexto);
                throw;
            }
        }

        public static async Task<T> ExecutarComTratamentoAsync<T>(Func<Task<T>> operacao, ILogger logger, string contexto = "Operação")
        {
            try
            {
                return await operacao();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro durante {Contexto}", contexto);
                throw;
            }
        }
    }
}