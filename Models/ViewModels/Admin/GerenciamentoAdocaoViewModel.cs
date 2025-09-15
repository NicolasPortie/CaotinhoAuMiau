using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.Enums;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class GerenciamentoAdocaoViewModel
    {
        public List<AdocaoAdminSummaryViewModel> Adocoes { get; set; } = new();
        public AdocaoStatisticsViewModel Statistics { get; set; } = new();
        public AdocaoFilterOptionsViewModel FilterOptions { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
        
        // Filtros aplicados
        public string? FiltroStatus { get; set; }
        public string? FiltroData { get; set; }
        public string? Pesquisa { get; set; }
        public string? OrdenarPor { get; set; } = "DataEnvio";
        public string? DirecaoOrdem { get; set; } = "Desc";
    }

    public class AdocaoAdminSummaryViewModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; }
        public DateTime? DataResposta { get; set; }
        public DateTime? DataAssinatura { get; set; }
        public DateTime? DataFinalizacao { get; set; }
        public DateTime? DataCriacaoContrato { get; set; }
        
        // Dados do Pet
        public int PetId { get; set; }
        public string PetNome { get; set; } = string.Empty;
        public string? PetImagem { get; set; }
        public Especie PetEspecie { get; set; }
        public string PetRaca { get; set; } = string.Empty;
        
        // Dados do Usuário
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public string UsuarioEmail { get; set; } = string.Empty;
        public string? UsuarioTelefone { get; set; }
        
        // Dados do Contrato
        public bool TemContrato { get; set; }
        public int? ContratoId { get; set; }
        public bool ContratoAssinado { get; set; }
        
        // Propriedades computadas
        public string StatusCssClass => Status.ToLower() switch
        {
            "aguardando contrato" => "aguardando-assinar-contrato",
            "aguardando buscar" => "aguardando-buscar",
            "finalizado" => "finalizado",
            "cancelado pela equipe" => "cancelado-caotinho",
            "cancelado - inatividade" => "cancelado-inatividade",
            "não assinou contrato" => "cancelado-nao-assinou-contrato",
            "não buscou no prazo" => "cancelado-prazo-vencido",
            // Compatibilidade com status antigos
            "aguardando assinar contrato" => "aguardando-assinar-contrato",
            "cancelado pelo caotinho" => "cancelado-caotinho",
            "cancelado por inatividade" => "cancelado-inatividade",
            _ => "desconhecido"
        };
        
        public string StatusTexto => Status switch
        {
            // Novos status dos enums
            "Aguardando Contrato" => "Aguardando Contrato",
            "Aguardando Buscar" => "Aguardando Buscar",
            "Finalizado" => "Finalizado", 
            "Cancelado pela Equipe" => "Cancelado pela Equipe",
            "Cancelado - Inatividade" => "Cancelado - Inatividade",
            "Não Assinou Contrato" => "Não Assinou Contrato",
            "Não Buscou no Prazo" => "Não Buscou no Prazo",
            // Compatibilidade com status antigos
            "Aguardando Assinar Contrato" => "Aguardando Contrato",
            "Cancelado Pelo Caotinho" => "Cancelado pela Equipe",
            "Cancelado por Inatividade" => "Cancelado - Inatividade",
            _ => Status
        };
        
        public string TempoProcessamento
        {
            get
            {
                var dataFinal = DataFinalizacao ?? DataResposta ?? DateTime.Now;
                var tempo = dataFinal - DataEnvio;
                
                if (tempo.Days > 0)
                    return $"{tempo.Days} dia(s)";
                if (tempo.Hours > 0)
                    return $"{tempo.Hours} hora(s)";
                return "Recente";
            }
        }

        // Propriedades para prazo de assinatura
        public bool TemPrazoAssinatura => Status == "Aguardando Contrato" || Status == "Aguardando Assinar Contrato";

        public int DiasRestantesParaAssinar
        {
            get
            {
                if (!TemPrazoAssinatura || !DataCriacaoContrato.HasValue)
                    return 0;

                var dataLimite = DataCriacaoContrato.Value.AddDays(3); // 3 dias corridos
                var diasRestantes = (dataLimite.Date - DateTime.Now.Date).Days;
                return Math.Max(0, diasRestantes);
            }
        }

        public DateTime? DataLimiteAssinatura
        {
            get
            {
                if (!DataCriacaoContrato.HasValue) return null;
                return DataCriacaoContrato.Value.AddDays(3); // 3 dias corridos
            }
        }

        public string TextoPrazoAssinatura
        {
            get
            {
                if (!TemPrazoAssinatura || !DataLimiteAssinatura.HasValue)
                    return "";

                if (DiasRestantesParaAssinar <= 0)
                    return "Prazo vencido";
                else if (DiasRestantesParaAssinar == 1)
                    return $"Último dia - {DataLimiteAssinatura.Value:dd/MM/yyyy}";
                else
                    return $"Até {DataLimiteAssinatura.Value:dd/MM/yyyy}";
            }
        }

        public string PetImagemUrl => !string.IsNullOrEmpty(PetImagem) 
            ? $"/imagens/pets/{PetImagem}" 
            : "/imagens/pets/pet-placeholder.jpg";
            
        public bool PodeAprovar => false; // Adoções já estão aprovadas
        public bool PodeRejeitar => false; // Adoções já estão aprovadas
        public bool PodeGerarContrato => false; // Contratos já são criados quando o formulário é aprovado
        public bool PodeFinalizar => Status == "Aguardando Buscar";
        public bool PodeCancelar => Status == "Aguardando Contrato" || Status == "Aguardando Buscar" || Status == "Aguardando Assinar Contrato";
        
        public int DiasRestantes
        {
            get
            {
                if (DataAssinatura == null || Status != "Aguardando Buscar") return 0;

                var dataLimite = CalcularDataLimiteRetirada(DataAssinatura.Value);
                var diasRestantes = (dataLimite.Date - DateTime.Now.Date).Days;
                return Math.Max(0, diasRestantes);
            }
        }

        public DateTime? DataLimiteRetirada
        {
            get
            {
                if (DataAssinatura == null) return null;
                return CalcularDataLimiteRetirada(DataAssinatura.Value);
            }
        }

        public bool PrazoVencido => DataLimiteRetirada.HasValue && DateTime.Now.Date > DataLimiteRetirada.Value.Date;

        private static DateTime CalcularDataLimiteRetirada(DateTime dataAssinatura)
        {
            var dataLimite = dataAssinatura.Date; // Garantir que é só a data
            var diasAdicionados = 0;

            // Começar a contar a partir do próximo dia útil
            while (diasAdicionados < 5)
            {
                dataLimite = dataLimite.AddDays(1);

                // Só conta se for dia útil (não sábado nem domingo)
                if (dataLimite.DayOfWeek != DayOfWeek.Saturday &&
                    dataLimite.DayOfWeek != DayOfWeek.Sunday)
                {
                    diasAdicionados++;
                }
            }

            return dataLimite;
        }
    }

    public class AdocaoStatisticsViewModel
    {
        public int TotalAdocoes { get; set; }
        public int AdocoesPendentes { get; set; }
        public int AdocoesAprovadas { get; set; }
        public int ContratoPendente { get; set; }
        public int ContratoAssinado { get; set; }
        public int AguardandoBusca { get; set; }
        public int AdocoesFinalizadas { get; set; }
        public int AdocoesRejeitadas { get; set; }
        public int AdocoesCanceladas { get; set; }
        
        public double TaxaAprovacao => TotalAdocoes > 0 ? (double)AdocoesAprovadas / TotalAdocoes * 100 : 0;
        public double TaxaFinalizacao => TotalAdocoes > 0 ? (double)AdocoesFinalizadas / TotalAdocoes * 100 : 0;
        
        public double TempoMedioProcessamento { get; set; } // Em dias
    }

    public class AdocaoFilterOptionsViewModel
    {
        public List<SelectOptionViewModel> StatusOptions { get; set; } = new();
        public List<SelectOptionViewModel> DataOptions { get; set; } = new();
        public List<SelectOptionViewModel> OrdenacaoOptions { get; set; } = new();
        
        public static AdocaoFilterOptionsViewModel Create()
        {
            return new AdocaoFilterOptionsViewModel
            {
                StatusOptions = new List<SelectOptionViewModel>
                {
                    new("", "Todos os status"),
                    new("Aguardando Assinar Contrato", "Aguardando Assinar Contrato"),
                    new("Aguardando Buscar", "Aguardando Buscar"),
                    new("Finalizado", "Finalizado"),
                    new("Cancelado Pelo Caotinho", "Cancelado Pelo Caotinho"),
                    new("Cancelado por Inatividade", "Cancelado por Inatividade")
                },
                DataOptions = new List<SelectOptionViewModel>
                {
                    new("", "Todas as datas"),
                    new("hoje", "Hoje"),
                    new("7dias", "Últimos 7 dias"),
                    new("30dias", "Últimos 30 dias")
                },
                OrdenacaoOptions = new List<SelectOptionViewModel>
                {
                    new("DataEnvio", "Data de Envio"),
                    new("Status", "Status"),
                    new("PetNome", "Nome do Pet"),
                    new("UsuarioNome", "Nome do Usuário")
                }
            };
        }
    }
    
    public class AdocaoDetalhesViewModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; }
        public DateTime? DataResposta { get; set; }
        public DateTime? DataAssinatura { get; set; }
        public DateTime? DataFinalizacao { get; set; }
        public string? MotivoRejeicao { get; set; }
        public string? ObservacoesAdmin { get; set; }
        
        // Pet
        public PetDetalhesAdocaoViewModel Pet { get; set; } = new();
        
        // Usuário  
        public UsuarioDetalhesAdocaoViewModel Usuario { get; set; } = new();
        
        // Formulário
        public FormularioDetalhesViewModel Formulario { get; set; } = new();
        
        // Contrato
        public ContratoDetalhesViewModel? Contrato { get; set; }
        
        // Histórico
        public List<HistoricoAdocaoViewModel> Historico { get; set; } = new();
    }

    public class PetDetalhesAdocaoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public Especie Especie { get; set; }
        public string Raca { get; set; } = string.Empty;
        public int Anos { get; set; }
        public int Meses { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string Porte { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? NomeArquivoImagem { get; set; }
        
        public string ImagemUrl => !string.IsNullOrEmpty(NomeArquivoImagem) 
            ? $"/imagens/pets/{NomeArquivoImagem}" 
            : "/imagens/pets/pet-placeholder.jpg";
            
        public string IdadeFormatada => $"{Anos} anos e {Meses} meses";
    }

    public class UsuarioDetalhesAdocaoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public DateTime DataCadastro { get; set; }
        public int TotalAdocoes { get; set; }
        public int AdocoesFinalizadas { get; set; }
        
        public string TempoComoUsuario
        {
            get
            {
                var tempo = DateTime.Now - DataCadastro;
                if (tempo.Days > 365)
                    return $"{tempo.Days / 365} ano(s)";
                if (tempo.Days > 30)
                    return $"{tempo.Days / 30} mês(es)";
                return $"{tempo.Days} dia(s)";
            }
        }
    }

    public class FormularioDetalhesViewModel
    {
        public string TipoResidencia { get; set; } = string.Empty;
        public int NumeroMoradores { get; set; }
        public string DescricaoMoradia { get; set; } = string.Empty;
        public string RendaMensal { get; set; } = string.Empty;
        public string CondicoesFinanceiras { get; set; } = string.Empty;
        public string TevePet { get; set; } = string.Empty;
        public string? ExperienciaAnterior { get; set; }
        public string EspacoAdequado { get; set; } = string.Empty;
        public string TempoDisponivel { get; set; } = string.Empty;
        public string PlanejamentoViagens { get; set; } = string.Empty;
        public string MotivacaoAdocao { get; set; } = string.Empty;
        public bool ConcordaTermos { get; set; }
    }

    public class ContratoDetalhesViewModel
    {
        public int Id { get; set; }
        public bool Assinado { get; set; }
        public DateTime? DataAssinatura { get; set; }
        public string? AssinaturaBase64 { get; set; }
        public DateTime DataVencimento { get; set; }
        
        public bool Vencido => !Assinado && DateTime.Now > DataVencimento;
        public int DiasRestantes => Assinado ? 0 : Math.Max(0, (DataVencimento - DateTime.Now).Days);
    }

    public class HistoricoAdocaoViewModel
    {
        public DateTime Data { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string? Detalhes { get; set; }
        public string UsuarioResponsavel { get; set; } = string.Empty;
        
        public string DataFormatada => Data.ToString("dd/MM/yyyy HH:mm");
    }
}