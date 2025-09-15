using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CaotinhoAuMiau.Services
{
    public class AssinaturaDigitalService
    {
        public class DadosAssinatura
        {
            public string? AssinaturaBase64 { get; set; }
            public DateTime DataAssinatura { get; set; }
            public string? Navegador { get; set; }
            public string? IpUsuario { get; set; }
            public int TamanhoAssinatura { get; set; }
        }

        public (bool valida, string mensagem) ValidarAssinatura(string assinaturaJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(assinaturaJson))
                {
                    return (false, "Assinatura não pode estar vazia.");
                }

                var dados = JsonSerializer.Deserialize<DadosAssinatura>(assinaturaJson);
                
                if (dados == null)
                {
                    return (false, "Formato de assinatura inválido.");
                }

                if (string.IsNullOrWhiteSpace(dados.AssinaturaBase64))
                {
                    return (false, "Assinatura digital é obrigatória.");
                }

                if (!EhBase64Valido(dados.AssinaturaBase64))
                {
                    return (false, "Formato de assinatura inválido.");
                }

                if (EhAssinaturaVazia(dados.AssinaturaBase64))
                {
                    return (false, "Por favor, desenhe sua assinatura no campo indicado.");
                }

                if (dados.TamanhoAssinatura < 100)
                {
                    return (false, "Assinatura muito simples. Por favor, faça uma assinatura mais detalhada.");
                }

                return (true, "Assinatura válida.");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao validar assinatura: {ex.Message}");
            }
        }

        public string CriarDadosAssinatura(string assinaturaBase64, string? navegador = null, string? ipUsuario = null)
        {
            var dados = new DadosAssinatura
            {
                AssinaturaBase64 = assinaturaBase64,
                DataAssinatura = DateTime.Now,
                Navegador = navegador,
                IpUsuario = ipUsuario,
                TamanhoAssinatura = CalcularTamanhoAssinatura(assinaturaBase64)
            };

            return JsonSerializer.Serialize(dados);
        }

        public string? ExtrairImagemBase64(string assinaturaJson)
        {
            try
            {
                var dados = JsonSerializer.Deserialize<DadosAssinatura>(assinaturaJson);
                return dados?.AssinaturaBase64;
            }
            catch
            {
                return null;
            }
        }

        public DateTime? ObterDataAssinatura(string assinaturaJson)
        {
            try
            {
                var dados = JsonSerializer.Deserialize<DadosAssinatura>(assinaturaJson);
                return dados?.DataAssinatura;
            }
            catch
            {
                return null;
            }
        }

        private bool EhBase64Valido(string base64String)
        {
            try
            {
                if (base64String.StartsWith("data:image/"))
                {
                    var commaIndex = base64String.IndexOf(',');
                    if (commaIndex >= 0)
                    {
                        base64String = base64String.Substring(commaIndex + 1);
                    }
                }

                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool EhAssinaturaVazia(string base64String)
        {
            try
            {
                if (base64String.Length < 500)
                {
                    return true;
                }

                var padroesVazios = new[]
                {
                    "iVBORw0KGgoAAAANSUhEUgAA",
                    "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
                };

                foreach (var padrao in padroesVazios)
                {
                    if (base64String.Contains(padrao))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return true;
            }
        }

        private int CalcularTamanhoAssinatura(string base64String)
        {
            try
            {
                return base64String.Length;
            }
            catch
            {
                return 0;
            }
        }

        public string GerarHashAssinatura(string assinaturaJson)
        {
            try
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(assinaturaJson));
                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }
    }
}