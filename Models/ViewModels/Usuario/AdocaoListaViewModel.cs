using System.Collections.Generic;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Models.ViewModels.Admin;

namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class AdocaoListaViewModel
    {
        public List<AdocaoUsuarioSummaryViewModel> Adocoes { get; set; } = new List<AdocaoUsuarioSummaryViewModel>();
        public AdocaoStatisticsUsuarioViewModel Statistics { get; set; } = new AdocaoStatisticsUsuarioViewModel();
        public AdocaoFilterOptionsUsuarioViewModel FilterOptions { get; set; } = new AdocaoFilterOptionsUsuarioViewModel();
        public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();
        
        public string UsuarioNome { get; set; } = string.Empty;
        public string? FotoPerfilUsuario { get; set; }
        
        public string? FiltroStatus { get; set; }
        public string? Pesquisa { get; set; }
        public string? OrdenarPor { get; set; } = "DataEnvio";
        public string? DirecaoOrdem { get; set; } = "Desc";
        
        public List<FormularioAdocao> Formularios => new List<FormularioAdocao>();
    }

    public class AdocaoUsuarioSummaryViewModel
    {
        public int Id { get; set; }
        public int? AdocaoId { get; set; }
        public StatusAdocao? StatusAdocao { get; set; }
        public StatusFormulario? StatusFormulario { get; set; }
        public DateTime DataEnvio { get; set; }
        public DateTime? DataResposta { get; set; }
        public DateTime? DataAssinatura { get; set; }
        public DateTime? DataFinalizacao { get; set; }
        public DateTime? DataCriacaoContrato { get; set; }
        
        public int PetId { get; set; }
        public string PetNome { get; set; } = string.Empty;
        public string? PetImagem { get; set; }
        public Especie PetEspecie { get; set; }
        public string PetRaca { get; set; } = string.Empty;
        public int PetIdade { get; set; }
        public int PetAnos { get; set; }
        public int PetMeses { get; set; }
        public SexoPet PetSexo { get; set; }
        public string? PetDescricao { get; set; }
        
        public string StatusTexto => StatusAdocao?.ObterValorMembroEnum() ?? StatusFormulario?.ObterValorMembroEnum() ?? "Não informado";
        
        public string Status => StatusTexto;
        
        
        public string StatusClass => StatusAdocao?.ObterCssClass() ?? StatusFormulario?.ObterCssClass() ?? "unknown";
        
        public string StatusIcon => StatusAdocao?.ObterIcone() ?? StatusFormulario?.ObterIcone() ?? "fas fa-question-circle";
        
        public string PetImagemUrl => !string.IsNullOrEmpty(PetImagem) 
            ? $"/imagens/pets/{PetImagem}" 
            : "/imagens/pets/pet-placeholder.jpg";
            
        public string PetIdadeTexto
        {
            get
            {
                if (PetAnos > 0 && PetMeses > 0)
                    return $"{PetAnos} {(PetAnos == 1 ? "ano" : "anos")} e {PetMeses} {(PetMeses == 1 ? "mês" : "meses")}";
                else if (PetAnos > 0)
                    return $"{PetAnos} {(PetAnos == 1 ? "ano" : "anos")}";
                else if (PetMeses > 0)
                    return $"{PetMeses} {(PetMeses == 1 ? "mês" : "meses")}";
                else
                    return "Idade não informada";
            }
        }
        public string PetSexoTexto => PetSexo == SexoPet.Macho ? "Macho" : "Fêmea";
        public string PetSexoIcon => PetSexo == SexoPet.Macho ? "fa-mars" : "fa-venus";
        
        public bool PodeVerDetalhes => true;
        public bool PodeCancelar => StatusAdocao?.Equals(Models.Enums.StatusAdocao.AguardandoAssinarContrato) == true;
        public bool PodeAssinarContrato => StatusAdocao?.Equals(Models.Enums.StatusAdocao.AguardandoAssinarContrato) == true;
        public bool TemPrazoRetirada => StatusAdocao?.Equals(Models.Enums.StatusAdocao.AguardandoBuscar) == true;
        public bool EstaFinalizada => StatusAdocao?.Equals(Models.Enums.StatusAdocao.Finalizado) == true;

        public bool TemPrazoAssinatura => StatusAdocao?.Equals(Models.Enums.StatusAdocao.AguardandoAssinarContrato) == true;

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

        // Cálculo de prazo (5 dias úteis após assinatura do contrato)
        public int DiasRestantesParaBuscar
        {
            get
            {
                if (!TemPrazoRetirada || !DataAssinatura.HasValue)
                    return 0;

                var dataLimite = CalcularDataLimiteRetirada(DataAssinatura.Value);
                var diasRestantes = (dataLimite.Date - DateTime.Now.Date).Days;
                return Math.Max(0, diasRestantes);
            }
        }

        public DateTime? DataLimiteRetirada
        {
            get
            {
                if (!DataAssinatura.HasValue) return null;
                return CalcularDataLimiteRetirada(DataAssinatura.Value);
            }
        }

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
        
        public bool PrazoVencido => TemPrazoRetirada && DiasRestantesParaBuscar <= 0;
        public bool PrazoUrgente => TemPrazoRetirada && DiasRestantesParaBuscar <= 2 && DiasRestantesParaBuscar > 0;
        
        public string TextoPrazo
        {
            get
            {
                // Prioridade 1: Aguardando assinatura do contrato
                if (TemPrazoAssinatura && DataLimiteAssinatura.HasValue)
                {
                    if (DiasRestantesParaAssinar == 1)
                        return $"Último dia para assinar - {DataLimiteAssinatura.Value:dd/MM/yyyy}";
                    else if (DiasRestantesParaAssinar > 1)
                        return $"Assinar até {DataLimiteAssinatura.Value:dd/MM/yyyy}";
                    else
                        return "Prazo de assinatura vencido";
                }

                // Prioridade 2: Aguardando buscar o pet
                if (TemPrazoRetirada && DataLimiteRetirada.HasValue && !PrazoVencido)
                {
                    if (DiasRestantesParaBuscar == 1)
                        return $"Último dia para buscar - {DataLimiteRetirada.Value:dd/MM/yyyy}";
                    else
                        return $"Buscar até {DataLimiteRetirada.Value:dd/MM/yyyy}";
                }

                return "";
            }
        }
        
        public string ClassePrazo
        {
            get
            {
                // Para prazo de assinatura
                if (TemPrazoAssinatura)
                {
                    if (DiasRestantesParaAssinar <= 0) return "vencido";
                    if (DiasRestantesParaAssinar <= 1) return "urgente";
                    return "warning";
                }

                // Para prazo de retirada
                if (TemPrazoRetirada)
                {
                    if (PrazoVencido) return "vencido";
                    if (PrazoUrgente) return "urgente";
                    return "";
                }

                return "";
            }
        }
        
        // CONSOLIDADO: Usando enums ao invés de strings confusas
        public bool EstaCancelada => StatusAdocao.HasValue && StatusAdocao.Value >= Models.Enums.StatusAdocao.CanceladoPeloCaotinho ||
                                    StatusFormulario?.Equals(Models.Enums.StatusFormulario.Negado) == true ||
                                    StatusFormulario?.Equals(Models.Enums.StatusFormulario.CanceladoPeloUsuario) == true ||
                                    StatusFormulario?.Equals(Models.Enums.StatusFormulario.CanceladoPorInatividade) == true;
    }

    public class AdocaoStatisticsUsuarioViewModel
    {
        public int TotalSolicitacoes { get; set; }
        public int EmAnalise { get; set; }
        public int Aprovadas { get; set; }
        public int Concluidas { get; set; }
        
        public string TotalSolicitacoesTexto => $"{TotalSolicitacoes} solicitaç{(TotalSolicitacoes == 1 ? "ão" : "ões")}";
        public string EmAnaliseTexto => $"{EmAnalise} em análise";
        public string AprovadasTexto => $"{Aprovadas} aprovada{(Aprovadas == 1 ? "" : "s")}";
        public string ConcluidasTexto => $"{Concluidas} concluída{(Concluidas == 1 ? "" : "s")}";
    }

    public class AdocaoFilterOptionsUsuarioViewModel
    {
        public List<SelectOptionViewModel> StatusOptions { get; set; } = new();
        public List<SelectOptionViewModel> OrdenacaoOptions { get; set; } = new();
        
        public static AdocaoFilterOptionsUsuarioViewModel Create()
        {
            return new AdocaoFilterOptionsUsuarioViewModel
            {
                StatusOptions = new List<SelectOptionViewModel>
                {
                    new("all", "Todos"),
                    new("Pendente", "Em Análise"),
                    new("Aprovado", "Aprovados"),
                    new("Finalizada", "Concluídas")
                },
                OrdenacaoOptions = new List<SelectOptionViewModel>
                {
                    new("recent", "Mais Recentes"),
                    new("oldest", "Mais Antigos"),
                    new("name", "Nome A-Z"),
                    new("status", "Por Status")
                }
            };
        }
    }
} 