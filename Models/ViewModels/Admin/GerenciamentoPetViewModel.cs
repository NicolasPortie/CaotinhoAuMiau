using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Services;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class GerenciamentoPetViewModel
    {
        public List<PetAdminSummaryViewModel> Pets { get; set; } = new();
        public PetStatisticsViewModel Statistics { get; set; } = new();
        public PetFilterOptionsViewModel FilterOptions { get; set; } = new();
        public PaginationViewModel Pagination { get; set; } = new();
        
        // Filtros aplicados
        public string? FiltroNome { get; set; }
        public Especie? FiltroEspecie { get; set; }
        public StatusPet? FiltroStatus { get; set; }
        public string? OrdenarPor { get; set; } = "Nome";
        public string? DirecaoOrdem { get; set; } = "Asc";
    }

    public class PetAdminSummaryViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public Especie Especie { get; set; }
        public string Raca { get; set; } = string.Empty;
        public int Anos { get; set; }
        public int Meses { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string Porte { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public StatusPet Status { get; set; }
        public string? NomeArquivoImagem { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAdocao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool TemAdocaoAtiva { get; set; }
        public string? NomeAdotante { get; set; }
        
        // Propriedades para compatibilidade com JavaScript
        public DateTime DataCriacao => DataCadastro;
        
        // Propriedades computadas
        public string ImagemUrl => !string.IsNullOrEmpty(NomeArquivoImagem) 
            ? $"/imagens/pets/{NomeArquivoImagem}" 
            : "/imagens/pets/pet-placeholder.jpg";
            
        public string IdadeFormatada => $"{Anos} anos e {Meses} meses";
        
        public string StatusCssClass => Status.ObterCssClass();
        
        public string StatusTexto => Status.ObterTexto();
        
        public string EspecieTexto => Especie.ObterTexto();
        
        public string TempoDesdeUltimaAtualizacao
        {
            get
            {
                var dataReferencia = DataAdocao ?? DataCadastro;
                var tempo = DateTime.Now - dataReferencia;
                
                if (tempo.Days > 0)
                    return $"{tempo.Days} dia(s) atrás";
                if (tempo.Hours > 0)
                    return $"{tempo.Hours} hora(s) atrás";
                return "Recente";
            }
        }
        
        public bool PodeEditar => Status.PodeEditar();
        public bool PodeExcluir => Status.PodeExcluir(TemAdocaoAtiva);
        public bool PodeAtivar => Status.PodeAtivar();
    }

    public class PetStatisticsViewModel
    {
        public int TotalPets { get; set; }
        public int TotalCachorros { get; set; }
        public int TotalGatos { get; set; }
        public int PetsDisponiveis { get; set; }
        public int PetsAdotados { get; set; }
        public int PetsEmProcesso { get; set; }
        public int PetsRascunho { get; set; }
        
        public double PercentualAdocao => TotalPets > 0 ? (double)PetsAdotados / TotalPets * 100 : 0;
        public double PercentualDisponiveis => TotalPets > 0 ? (double)PetsDisponiveis / TotalPets * 100 : 0;
    }

    public class PetFilterOptionsViewModel
    {
        public List<SelectOptionViewModel> Especies { get; set; } = new();
        public List<SelectOptionViewModel> StatusOptions { get; set; } = new();
        public List<SelectOptionViewModel> OrdenacaoOptions { get; set; } = new();
        
        public static PetFilterOptionsViewModel Create()
        {
            return new PetFilterOptionsViewModel
            {
                Especies = EnumService.ObterOpcoesEspecie(),
                StatusOptions = EnumService.ObterOpcoesStatusPet(),
                OrdenacaoOptions = EnumService.ObterOpcoesOrdenacao()
            };
        }
    }

    public class SelectOptionViewModel
    {
        public string Value { get; set; }
        public string Text { get; set; }
        
        public SelectOptionViewModel(string value, string text)
        {
            Value = value;
            Text = text;
        }
    }

    public class PaginationViewModel
    {
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalItens { get; set; }
        public int ItensPorPagina { get; set; } = 20;
        
        public bool TemPaginaAnterior => PaginaAtual > 1;
        public bool TemProximaPagina => PaginaAtual < TotalPaginas;
        public int PaginaAnterior => Math.Max(1, PaginaAtual - 1);
        public int ProximaPagina => Math.Min(TotalPaginas, PaginaAtual + 1);
        
        public List<int> PaginasVisiveis
        {
            get
            {
                var paginas = new List<int>();
                var inicio = Math.Max(1, PaginaAtual - 2);
                var fim = Math.Min(TotalPaginas, PaginaAtual + 2);
                
                for (int i = inicio; i <= fim; i++)
                {
                    paginas.Add(i);
                }
                
                return paginas;
            }
        }
    }
}