using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class LogsViewModel
    {
        public List<Log> Logs { get; set; } = new();
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? TipoAcaoSelecionado { get; set; }
        public string? CategoriaSelecionada { get; set; }
        public string? NivelSeveridadeSelecionado { get; set; }
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalLogs { get; set; }
        public int ItensPorPagina { get; set; } = 20;

        // Opções disponíveis nos dropdowns de filtro
        public List<string> TiposAcao { get; set; } = new();
        public List<string> Categorias { get; set; } = new();
        public List<string> NiveisSeveridade { get; set; } = new();

        // Dados numéricos para exibir no painel
        public Dictionary<string, int> Estatisticas { get; set; } = new();
    }
}