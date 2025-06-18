using System.Collections.Generic;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class AdocaoListaViewModel
    {
        public List<FormularioAdocao> Formularios { get; set; } = new List<FormularioAdocao>();
        public int PaginaAtual { get; set; }
        public int TotalItens { get; set; }
        public int ItensPorPagina { get; set; }
        public int TotalPaginas => (TotalItens + ItensPorPagina - 1) / ItensPorPagina;
    }
} 