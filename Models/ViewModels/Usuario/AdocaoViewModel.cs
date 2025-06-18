using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class AdocaoViewModel
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public Pet Pet { get; set; } = null!;
        public int UsuarioId { get; set; }
        public CaotinhoAuMiau.Models.Usuario Usuario { get; set; } = null!;
        public DateTime DataEnvio { get; set; } = DateTime.Now;
        public DateTime? DataResposta { get; set; }
        public string Status { get; set; } = "Pendente";
        public string? ObservacaoAdminFormulario { get; set; }
        public string? ObservacoesCancelamento { get; set; }

        [Required(ErrorMessage = "O campo Espaço Adequado é obrigatório")]
        public string EspacoAdequado { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Experiência Anterior é obrigatório")]
        public string ExperienciaAnterior { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Motivação da Adoção é obrigatório")]
        public string MotivacaoAdocao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Condições Financeiras é obrigatório")]
        public string CondicoesFinanceiras { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Planejamento de Viagens é obrigatório")]
        public string PlanejamentoViagens { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo Renda Mensal é obrigatório")]
        [Range(1, double.MaxValue, ErrorMessage = "A renda mensal deve ser maior que zero")]
        public decimal RendaMensal { get; set; }

        [Required(ErrorMessage = "O campo Número de Moradores é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "O número de moradores deve ser maior que zero")]
        public int NumeroMoradores { get; set; }

        [Required(ErrorMessage = "O campo Descrição da Moradia é obrigatório")]
        public string DescricaoMoradia { get; set; } = string.Empty;

        public List<AdocaoViewModel> Adocoes { get; set; } = new List<AdocaoViewModel>();
        public List<FormularioAdocao> FormulariosPendentes { get; set; } = new List<FormularioAdocao>();
        
        public int IndicePagina { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalItens { get; set; } = 0;
        public bool TemPaginaAnterior => IndicePagina > 1;
        public bool TemProximaPagina => IndicePagina < TotalPaginas;
        public FormularioAdocao ToModel()
        {
            var formulario = new FormularioAdocao
            {
                Id = Id,
                PetId = PetId,
                UsuarioId = UsuarioId,
                DataEnvio = DataEnvio,
                DataResposta = DataResposta,
                Status = Status,
                ObservacaoAdminFormulario = ObservacaoAdminFormulario,
                ObservacoesCancelamento = ObservacoesCancelamento,
                EspacoAdequado = EspacoAdequado,
                ExperienciaAnterior = ExperienciaAnterior,
                MotivacaoAdocao = MotivacaoAdocao,
                CondicoesFinanceiras = CondicoesFinanceiras,
                PlanejamentoViagens = PlanejamentoViagens,
                RendaMensal = RendaMensal,
                NumeroMoradores = NumeroMoradores,
                DescricaoMoradia = DescricaoMoradia
            };

            return formulario;
        }

        public static AdocaoViewModel Criar(FormularioAdocao formulario)
        {
            return new AdocaoViewModel
            {
                Id = formulario.Id,
                PetId = formulario.PetId,
                UsuarioId = formulario.UsuarioId,
                DataEnvio = formulario.DataEnvio,
                DataResposta = formulario.DataResposta,
                Status = formulario.Status,
                ObservacaoAdminFormulario = formulario.ObservacaoAdminFormulario,
                ObservacoesCancelamento = formulario.ObservacoesCancelamento,
                EspacoAdequado = formulario.EspacoAdequado,
                ExperienciaAnterior = formulario.ExperienciaAnterior,
                MotivacaoAdocao = formulario.MotivacaoAdocao,
                CondicoesFinanceiras = formulario.CondicoesFinanceiras,
                PlanejamentoViagens = formulario.PlanejamentoViagens,
                RendaMensal = formulario.RendaMensal,
                NumeroMoradores = formulario.NumeroMoradores,
                DescricaoMoradia = formulario.DescricaoMoradia
            };
        }

        public static AdocaoViewModel CriarParaListagem()
        {
            return new AdocaoViewModel
            {
                Adocoes = new List<AdocaoViewModel>(),
                FormulariosPendentes = new List<FormularioAdocao>(),
                IndicePagina = 1,
                TotalPaginas = 1,
                TotalItens = 0
            };
        }
    }
} 