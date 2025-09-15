using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Utils;
using CaotinhoAuMiau.Models.ViewModels.Admin;

namespace CaotinhoAuMiau.Services
{
    public static class EnumService
    {
        /// <summary>
        /// Obtém opções de filtro para Status de Pet
        /// </summary>
        public static List<SelectOptionViewModel> ObterOpcoesStatusPet()
        {
            return new List<SelectOptionViewModel>
            {
                new("", "Todos os status"),
                new(StatusPet.Disponivel.ToString(), StatusPet.Disponivel.ObterTexto()),
                new(StatusPet.EmProcesso.ToString(), StatusPet.EmProcesso.ObterTexto()),
                new(StatusPet.Adotado.ToString(), StatusPet.Adotado.ObterTexto()),
                new(StatusPet.Rascunho.ToString(), StatusPet.Rascunho.ObterTexto())
            };
        }

        /// <summary>
        /// Obtém opções de filtro para Status de Adoção
        /// </summary>
        public static List<SelectOptionViewModel> ObterOpcoesStatusAdocao()
        {
            return new List<SelectOptionViewModel>
            {
                new("", "Todos os status"),
                new(StatusAdocao.AguardandoAssinarContrato.ToString(), StatusAdocao.AguardandoAssinarContrato.ObterTexto()),
                new(StatusAdocao.AguardandoBuscar.ToString(), StatusAdocao.AguardandoBuscar.ObterTexto()),
                new(StatusAdocao.Finalizado.ToString(), StatusAdocao.Finalizado.ObterTexto()),
                new(StatusAdocao.CanceladoPeloCaotinho.ToString(), StatusAdocao.CanceladoPeloCaotinho.ObterTexto()),
                new(StatusAdocao.CanceladoPorPrazoVencido.ToString(), StatusAdocao.CanceladoPorPrazoVencido.ObterTexto())
            };
        }

        /// <summary>
        /// Obtém opções de filtro para Status de Formulário
        /// </summary>
        public static List<SelectOptionViewModel> ObterOpcoesStatusFormulario()
        {
            return new List<SelectOptionViewModel>
            {
                new("", "Todos os status"),
                new(StatusFormulario.Pendente.ToString(), StatusFormulario.Pendente.ObterTexto()),
                new(StatusFormulario.EmAnalise.ToString(), StatusFormulario.EmAnalise.ObterTexto()),
                new(StatusFormulario.Aprovado.ToString(), StatusFormulario.Aprovado.ObterTexto()),
                new(StatusFormulario.Negado.ToString(), StatusFormulario.Negado.ObterTexto()),
                new(StatusFormulario.CanceladoPeloUsuario.ToString(), StatusFormulario.CanceladoPeloUsuario.ObterTexto()),
                new(StatusFormulario.CanceladoPorInatividade.ToString(), StatusFormulario.CanceladoPorInatividade.ObterTexto())
            };
        }

        /// <summary>
        /// Obtém opções de filtro para Espécie
        /// </summary>
        public static List<SelectOptionViewModel> ObterOpcoesEspecie()
        {
            return new List<SelectOptionViewModel>
            {
                new("", "Todas as espécies"),
                new(Especie.Cao.ToString(), Especie.Cao.ObterTermoAmigavel()),
                new(Especie.Felino.ToString(), Especie.Felino.ObterTermoAmigavel())
            };
        }

        /// <summary>
        /// Obtém opções de ordenação padrão
        /// </summary>
        public static List<SelectOptionViewModel> ObterOpcoesOrdenacao()
        {
            return new List<SelectOptionViewModel>
            {
                new("Nome", "Nome"),
                new("DataCadastro", "Data de Cadastro"),
                new("DataCriacao", "Data de Criação"),
                new("Status", "Status"),
                new("Especie", "Espécie")
            };
        }

        /// <summary>
        /// Converte string para enum de forma segura
        /// </summary>
        public static T? ConverterStringParaEnum<T>(string? valor) where T : struct, Enum
        {
            if (string.IsNullOrEmpty(valor))
                return null;

            return Enum.TryParse<T>(valor, out var resultado) ? resultado : null;
        }

        /// <summary>
        /// Verifica se uma string é um valor válido para o enum
        /// </summary>
        public static bool EhValorValidoEnum<T>(string? valor) where T : struct, Enum
        {
            return !string.IsNullOrEmpty(valor) && Enum.TryParse<T>(valor, out _);
        }
    }
}