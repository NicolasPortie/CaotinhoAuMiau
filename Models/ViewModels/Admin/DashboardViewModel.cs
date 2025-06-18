using System.Collections.Generic;
using CaotinhoAuMiau.Models.ViewModels.Usuario;
using CaotinhoAuMiau.Models.ViewModels.Comuns;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public List<Models.ViewModels.Usuario.AdocaoViewModel> Formularios { get; set; } = new List<Models.ViewModels.Usuario.AdocaoViewModel>();
        public EstatisticasViewModel Estatisticas { get; set; } = new EstatisticasViewModel();
    }

    public class EstatisticasViewModel
    {
        public int TotalFormularios { get; set; }
        public int FormulariosPendentes { get; set; }
        public int FormulariosAprovados { get; set; }
        public int FormulariosReprovados { get; set; }
        
        public int TotalPets { get; set; }
        public int PetsAdotados { get; set; }
        public int TotalCachorros { get; set; }
        public int TotalGatos { get; set; }
        public int CachorrosAdotados { get; set; }
        public int GatosAdotados { get; set; }
        public int PetsEmProcesso { get; set; }
        
        public int TotalUsuarios { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalAdotantes { get; set; }
        
        public int PetsDisponiveis { get; set; }
        public int FormulariosPendentesHoje { get; set; }
        public int PetsAguardandoRetirada { get; set; }
        public int AdocoesFinalizadas { get; set; }

        public List<string> MesesAdocoes { get; set; } = new List<string>();
        public List<int> AdocoesPorMes { get; set; } = new List<int>();
        public List<int> UsuariosPorMes { get; set; } = new List<int>();
        public List<int> TotalUsuariosAcumulados { get; set; } = new List<int>();
    }
} 