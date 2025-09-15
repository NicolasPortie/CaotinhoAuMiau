using System;
using System.Collections.Generic;

namespace CaotinhoAuMiau.Models.ViewModels.Admin
{
    public class GerenciamentoUsuariosViewModel
    {
        public List<UsuarioResumoViewModel> Usuarios { get; set; } = new();
        
        // Filtros
        public string FiltroStatus { get; set; } = "todos";
        public string FiltroTipo { get; set; } = "todos";
        public string Pesquisa { get; set; } = string.Empty;
        
        // Paginação
        public int PaginaAtual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalItens { get; set; } = 0;
        public int ItensPorPagina { get; set; } = 20;
        public bool TemPaginaAnterior => PaginaAtual > 1;
        public bool TemProximaPagina => PaginaAtual < TotalPaginas;
        
        // Estatísticas
        public int TotalUsuarios { get; set; }
        public int UsuariosAtivos { get; set; }
        public int UsuariosQuarentena { get; set; }
        public int UsuariosViolacoes { get; set; }
        public int UsuariosInativos => TotalUsuarios - UsuariosAtivos;
        public double PercentualAtivos => TotalUsuarios > 0 ? (double)UsuariosAtivos / TotalUsuarios * 100 : 0;
    }

    public class UsuarioResumoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? UltimoAcesso { get; set; }
        public string? FotoPerfil { get; set; }
        public bool EmQuarentena { get; set; }
        public DateTime? FimQuarentena { get; set; }
        public int NumeroViolacoes { get; set; }
        public int TotalAdocoes { get; set; }
        public int AdocoesFinalizadas { get; set; }
        
        // Dados completos (apenas para detalhes)
        public string? EnderecoCompleto { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        
        // Propriedades calculadas
        public string StatusTexto => EmQuarentena ? "Em Quarentena" : 
                                   Ativo ? "Ativo" : "Inativo";
        public string StatusCssClass => EmQuarentena ? "status-quarentena" : 
                                       Ativo ? "status-ativo" : "status-inativo";
        public bool QuarentenaAtiva => EmQuarentena && FimQuarentena.HasValue && DateTime.Now < FimQuarentena.Value;
        public int? DiasRestantesQuarentena => QuarentenaAtiva && FimQuarentena.HasValue 
            ? Math.Max(0, (int)(FimQuarentena.Value - DateTime.Now).TotalDays + 1) 
            : null;
        
        // Dados mascarados para listagem (proteção de dados)
        public string CPFMascarado => CPF.Length == 11 
            ? $"{CPF.Substring(0, 3)}.***.**-{CPF.Substring(9, 2)}"
            : "***.***.***-**";
        public string EmailMascarado 
        {
            get
            {
                if (string.IsNullOrEmpty(Email) || !Email.Contains("@"))
                    return "***@***.***";
                
                var parts = Email.Split('@');
                var localPart = parts[0];
                var domainPart = parts[1];
                
                if (localPart.Length <= 2)
                    return $"***@{domainPart}";
                
                return $"{localPart.Substring(0, 2)}***@{domainPart}";
            }
        }
        public string? TelefoneMascarado 
        {
            get
            {
                if (string.IsNullOrEmpty(Telefone) || Telefone.Length < 10)
                    return "(**) ****-****";
                
                if (Telefone.Length == 11) // Celular
                    return $"({Telefone.Substring(0, 2)}) {Telefone.Substring(2, 1)}****-{Telefone.Substring(7, 4)}";
                else // Fixo
                    return $"({Telefone.Substring(0, 2)}) ****-{Telefone.Substring(6, 4)}";
            }
        }
        public string LocalizacaoResumo => !string.IsNullOrEmpty(Cidade) && !string.IsNullOrEmpty(Estado) 
            ? $"{Cidade}/{Estado}" : "Não informado";
        
        // Dados completos (formatados)
        public string CPFFormatado => CPF.Length == 11 
            ? $"{CPF.Substring(0, 3)}.{CPF.Substring(3, 3)}.{CPF.Substring(6, 3)}-{CPF.Substring(9, 2)}"
            : CPF;
        public string UltimoAcessoTexto => UltimoAcesso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca acessou";
        public bool UsuarioProblematico => NumeroViolacoes > 2 || !Ativo;
    }

    public class DetalhesUsuarioViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public bool EmailVerificado { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? UltimoAcesso { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? FotoPerfil { get; set; }
        
        // Endereço
        public string CEP { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        
        // Informações administrativas
        public bool EmQuarentena { get; set; }
        public DateTime? InicioQuarentena { get; set; }
        public DateTime? FimQuarentena { get; set; }
        public string? MotivoQuarentena { get; set; }
        public string? JustificativaRemocaoQuarentena { get; set; }
        public string? ObservacoesAdministrativas { get; set; }
        public DateTime? DataUltimaBloqueio { get; set; }
        public int NumeroViolacoes { get; set; }
        public bool RequererAprovacaoManual { get; set; }
        
        // Estatísticas
        public int TotalAdocoes { get; set; }
        public int AdocoesFinalizadas { get; set; }
        public int AdocoesCanceladas { get; set; }
        public int TotalFormularios { get; set; }
        
        // Propriedades calculadas
        public string EnderecoCompleto => $"{Logradouro}, {Numero}" + 
                                         (!string.IsNullOrEmpty(Complemento) ? $", {Complemento}" : "") + 
                                         $" - {Bairro}, {Cidade}/{Estado} - CEP: {CEP}";
        public string CPFFormatado => CPF.Length == 11 
            ? $"{CPF.Substring(0, 3)}.{CPF.Substring(3, 3)}.{CPF.Substring(6, 3)}-{CPF.Substring(9, 2)}"
            : CPF;
        public bool QuarentenaAtiva => EmQuarentena && FimQuarentena.HasValue && DateTime.Now < FimQuarentena.Value;
        public int? DiasRestantesQuarentena => QuarentenaAtiva && FimQuarentena.HasValue 
            ? Math.Max(0, (int)(FimQuarentena.Value - DateTime.Now).TotalDays + 1) 
            : null;
        public double TaxaSucessoAdocao => TotalAdocoes > 0 ? (double)AdocoesFinalizadas / TotalAdocoes * 100 : 0;
        public string ClassificacaoUsuario => NumeroViolacoes == 0 ? "Exemplar" :
                                             NumeroViolacoes <= 2 ? "Confiável" :
                                             NumeroViolacoes <= 5 ? "Atenção" : "Problemático";
        public string StatusGeral => !Ativo ? "Desativado" :
                                    QuarentenaAtiva ? "Em Quarentena" :
                                    NumeroViolacoes > 5 ? "Alto Risco" :
                                    "Normal";
    }
}