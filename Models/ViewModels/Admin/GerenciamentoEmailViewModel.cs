using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class GerenciamentoEmailViewModel
    {
        public ConfiguracaoEmail Configuracao { get; set; } = new();
        public EmailStatisticsViewModel Statistics { get; set; } = new();
        public string? EmailTeste { get; set; }
        public string? MensagemTeste { get; set; }
        public bool TesteRealizado { get; set; }
        public bool TesteSucesso { get; set; }
    }

    public class EmailStatisticsViewModel
    {
        public int TotalEnviados { get; set; }
        public int TotalErros { get; set; }
        public int EmailsHoje { get; set; }
        public int EmailsEstaSemana { get; set; }
        public double TaxaSucesso { get; set; }
        public List<EmailTipoStatistic> EstatisticasPorTipo { get; set; } = new();
        public List<EmailGraficoData> DadosGrafico { get; set; } = new();
        
        public string TotalEnviadosTexto => $"{TotalEnviados} email{(TotalEnviados == 1 ? "" : "s")}";
        public string TotalErrosTexto => $"{TotalErros} erro{(TotalErros == 1 ? "" : "s")}";
        public string EmailsHojeTexto => $"{EmailsHoje} hoje";
        public string TaxaSucessoTexto => $"{TaxaSucesso:F1}%";
    }

    public class EmailTipoStatistic
    {
        public string Tipo { get; set; } = string.Empty;
        public int Total { get; set; }
        
        public string TipoFormatado => Tipo switch
        {
            "FormularioAprovado" => "Formulário Aprovado",
            "ContratoDisponivel" => "Contrato Disponível", 
            "LembretePrazo" => "Lembrete de Prazo",
            "PrazoVencido" => "Prazo Vencido",
            "Teste" => "Teste",
            _ => Tipo
        };
    }

    public class EmailGraficoData
    {
        public DateTime Data { get; set; }
        public int Total { get; set; }
        
        public string DataFormatada => Data.ToString("dd/MM");
    }

    public class TesteEmailFormViewModel
    {
        public string EmailTeste { get; set; } = string.Empty;
        public bool UsarConfiguracaoSalva { get; set; } = true;
        
        // Para teste direto
        public string? ServidorSmtp { get; set; }
        public int? Porta { get; set; }
        public string? EmailRemetente { get; set; }
        public string? NomeRemetente { get; set; }
        public string? Senha { get; set; }
        public bool? UsarSsl { get; set; }
    }
}