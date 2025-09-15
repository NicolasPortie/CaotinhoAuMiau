namespace CaotinhoAuMiau.Models.ViewModels.Home
{
    public class SobreViewModel
    {
        public SobreStatisticsViewModel Statistics { get; set; } = new();
    }

    public class SobreStatisticsViewModel
    {
        public int PetsAdotados { get; set; }
        public int PetsDisponiveis { get; set; }
        public int TotalUsuarios { get; set; }
        public int Formularios { get; set; }
        public int TotalCachorros { get; set; }
        public int TotalGatos { get; set; }
        public int PetsEmProcesso { get; set; }
        
        // Formatação dos números e textos para mostrar na tela
        public string PetsAdotadosTexto => PetsAdotados.ToString("N0");
        public string PetsDisponiveisTexto => PetsDisponiveis.ToString("N0");
        public string TotalUsuariosTexto => TotalUsuarios.ToString("N0");
        public string FormulariosTexto => Formularios.ToString("N0");
        
        public string PetsAdotadosLabel => PetsAdotados == 1 ? "Pet Adotado" : "Pets Adotados";
        public string PetsDisponiveisLabel => PetsDisponiveis == 1 ? "Pet Disponível" : "Pets Disponíveis";
        public string TotalUsuariosLabel => TotalUsuarios == 1 ? "Usuário Cadastrado" : "Usuários Cadastrados";
        public string FormulariosLabel => Formularios == 1 ? "Formulário Processado" : "Formulários Processados";
        
        public double TaxaAdocao => PetsAdotados + PetsDisponiveis > 0 ? 
            (double)PetsAdotados / (PetsAdotados + PetsDisponiveis) * 100 : 0;
        public string TaxaAdocaoTexto => $"{TaxaAdocao:F1}%";
        
        public int TotalPets => PetsAdotados + PetsDisponiveis + PetsEmProcesso;
        public string TotalPetsTexto => TotalPets.ToString("N0");
    }
}