using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class ContratoUsuarioViewModel
    {
        public int ContratoId { get; set; }
        public int AdocaoId { get; set; }
        public string StatusContrato { get; set; } = string.Empty;
        public DateTime? DataAssinatura { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataVencimento { get; set; }
        public bool EstaAssinado { get; set; }
        public bool EstaPendente { get; set; }
        public bool EstaExpirado { get; set; }
        public string ConteudoContrato { get; set; } = string.Empty;
        public string? AssinaturaUsuario { get; set; }
        public bool PodeAssinar { get; set; }
        public string? MotivoNaoPodeAssinar { get; set; }

        // Dados do Pet
        public PetContratoUsuarioViewModel Pet { get; set; } = new();

        // Dados do Usuário Logado
        public UsuarioContratoLogadoViewModel Usuario { get; set; } = new();

        // Dados da Adoção
        public AdocaoContratoViewModel Adocao { get; set; } = new();

        // Propriedades computadas
        public string StatusTexto => EstaAssinado ? "Contrato Assinado" : 
                                   EstaPendente ? "Aguardando Assinatura" : 
                                   "Contrato Expirado";
        
        public string StatusCssClass => EstaAssinado ? "status-assinado" : 
                                      EstaPendente ? "status-pendente" : 
                                      "status-expirado";
        
        public string DataAssinaturaFormatada => DataAssinatura?.ToString("dd/MM/yyyy HH:mm") ?? "";
        
        public string DataVencimentoFormatada => DataVencimento?.ToString("dd/MM/yyyy HH:mm") ?? "";
        
        public int DiasRestantes => DataVencimento.HasValue 
            ? Math.Max(0, (DataVencimento.Value - DateTime.Now).Days)
            : 0;
        
        public bool VenceEm24Horas => DiasRestantes <= 1 && EstaPendente;
        
        public string ProgressoAssinatura => EstaAssinado ? "100" : EstaPendente ? "50" : "0";
    }

    public class PetContratoUsuarioViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string Raca { get; set; } = string.Empty;
        public int Anos { get; set; }
        public int Meses { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string? NomeArquivoImagem { get; set; }
        public string ImagemUrl => !string.IsNullOrEmpty(NomeArquivoImagem) 
            ? $"/imagens/pets/{NomeArquivoImagem}" 
            : "/imagens/pets/pet-placeholder.jpg";
        public string IdadeFormatada => $"{Anos} anos e {Meses} meses";
    }

    public class UsuarioContratoLogadoViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? FotoPerfil { get; set; }
        public string FotoUrl => !string.IsNullOrEmpty(FotoPerfil) 
            ? $"/imagens/perfil/{FotoPerfil}" 
            : "";
        public string InicialNome => !string.IsNullOrEmpty(Nome) ? Nome[0].ToString().ToUpper() : "U";
        public bool PossuiFoto => !string.IsNullOrEmpty(FotoPerfil);
    }

    public class AdocaoContratoViewModel
    {
        public DateTime DataEnvio { get; set; }
        public DateTime? DataResposta { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DataEnvioFormatada => DataEnvio.ToString("dd/MM/yyyy");
        public string DataRespostaFormatada => DataResposta?.ToString("dd/MM/yyyy") ?? "";
    }
}