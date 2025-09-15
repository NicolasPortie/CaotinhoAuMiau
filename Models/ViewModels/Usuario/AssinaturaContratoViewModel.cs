namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class AssinaturaContratoViewModel
    {
        public int ContratoId { get; set; }
        public int AdocaoId { get; set; }
        public string ConteudoContrato { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime DataVencimento { get; set; }
        public bool EstaPendente { get; set; }
        public bool EstaExpirado { get; set; }
        public int DiasRestantes { get; set; }
        public int HorasRestantes { get; set; }
        public bool UrgentePrazo => DiasRestantes <= 1;

        // Dados do Pet
        public PetAssinaturaViewModel Pet { get; set; } = new();

        // Dados do Usuário
        public UsuarioAssinaturaViewModel Usuario { get; set; } = new();

        // Propriedades computadas
        public string TempoRestanteTexto
        {
            get
            {
                if (EstaExpirado)
                    return "Contrato expirado";
                
                if (DiasRestantes > 1)
                    return $"{DiasRestantes} dias restantes";
                
                if (DiasRestantes == 1)
                    return "Último dia para assinar";
                
                if (HorasRestantes > 1)
                    return $"{HorasRestantes} horas restantes";
                
                return "Menos de 1 hora restante!";
            }
        }

        public string CorPrazo => EstaExpirado ? "danger" : 
                                 DiasRestantes <= 1 ? "warning" : 
                                 "success";

        public string IconePrazo => EstaExpirado ? "fas fa-times-circle" : 
                                   DiasRestantes <= 1 ? "fas fa-exclamation-triangle" : 
                                   "fas fa-clock";
    }

    public class PetAssinaturaViewModel
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
        public string DescricaoCompleta => $"{Nome} - {Especie}, {IdadeFormatada}";
    }

    public class UsuarioAssinaturaViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? CPF { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
    }
}