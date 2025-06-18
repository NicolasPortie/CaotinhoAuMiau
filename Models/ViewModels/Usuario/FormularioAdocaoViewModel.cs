using System;
using System.ComponentModel.DataAnnotations;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Models.ViewModels.Usuario
{
    public class FormularioAdocaoViewModel
    {
        public int PetId { get; set; }
        public string? PetNome { get; set; }
        
        public Pet? Pet { get; set; }
        
        public CaotinhoAuMiau.Models.Usuario? Usuario { get; set; }
        
        [Required(ErrorMessage = "O campo Tipo de Residência é obrigatório")]
        public string TipoResidencia { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Por favor, informe a quantidade de moradores")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade de moradores deve ser maior que zero")]
        public int NumeroMoradores { get; set; }
        
        [Required(ErrorMessage = "Por favor, descreva sua moradia")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string DescricaoMoradia { get; set; } = string.Empty;
        
        public bool TemQuintal { get; set; }
        public bool TemVaranda { get; set; }
        public bool TemRedeProtecao { get; set; }
        public bool TemAreaExterna { get; set; }
        
        [Required(ErrorMessage = "Por favor, informe sua renda mensal")]
        [Range(1, double.MaxValue, ErrorMessage = "A renda mensal deve ser maior que zero")]
        public decimal RendaMensal { get; set; }
        
        [Required(ErrorMessage = "Por favor, informe como planeja arcar com os custos do pet")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string CondicoesFinanceiras { get; set; } = string.Empty;
        
        public string TevePet { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Por favor, conte sobre sua experiência com pets")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string ExperienciaAnterior { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Por favor, descreva o espaço disponível para o pet")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string EspacoAdequado { get; set; } = string.Empty;
        
        public int TempoDisponivel { get; set; }
        
        [Required(ErrorMessage = "Por favor, informe o que fará com o pet quando precisar viajar")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        public string PlanejamentoViagens { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Por favor, informe sua motivação para adotar este pet")]
        [StringLength(500, ErrorMessage = "A motivação deve ter no máximo 500 caracteres")]
        public string MotivacaoAdocao { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Você deve aceitar os termos e condições")]
        public bool ConcordaTermos { get; set; }
        
        public FormularioAdocao ToModel()
        {
            string caracteristicasResidencia = "";
            if (TemQuintal) caracteristicasResidencia += "Quintal; ";
            if (TemVaranda) caracteristicasResidencia += "Varanda; ";
            if (TemRedeProtecao) caracteristicasResidencia += "Rede de proteção; ";
            if (TemAreaExterna) caracteristicasResidencia += "Área externa; ";
            
            return new FormularioAdocao
            {
                PetId = PetId,
                UsuarioId = Usuario?.Id ?? 0,
                DescricaoMoradia = $"Tipo: {TipoResidencia}. {DescricaoMoradia}. Características: {caracteristicasResidencia}",
                NumeroMoradores = NumeroMoradores,
                RendaMensal = RendaMensal,
                CondicoesFinanceiras = CondicoesFinanceiras,
                ExperienciaAnterior = $"Teve pet antes: {TevePet}. {ExperienciaAnterior}. Tempo disponível: {TempoDisponivel}h por dia.",
                EspacoAdequado = EspacoAdequado,
                PlanejamentoViagens = PlanejamentoViagens,
                MotivacaoAdocao = MotivacaoAdocao,
                Status = "Pendente",
                DataEnvio = DateTime.Now
            };
        }
        
        public static FormularioAdocaoViewModel Criar(Pet pet, CaotinhoAuMiau.Models.Usuario usuario)
        {
            return new FormularioAdocaoViewModel
            {
                PetId = pet.Id,
                PetNome = pet.Nome,
                Pet = pet,
                Usuario = usuario
            };
        }
    }
}