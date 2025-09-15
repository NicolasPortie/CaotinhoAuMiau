using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Services
{
    public class HistoricoAdocaoService
    {
        private readonly ApplicationDbContext _contexto;

        public HistoricoAdocaoService(ApplicationDbContext contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public async Task<List<HistoricoAdocaoItem>> ObterPorUsuarioAsync(int usuarioId, bool apenasFinalizadas = false)
        {
            var query = _contexto.Adocoes
                .Include(a => a.Pet)
                .Where(a => a.UsuarioId == usuarioId);

            if (apenasFinalizadas)
            {
                query = query.Where(a => a.Status == Models.Enums.StatusAdocao.Finalizado)
                             .OrderByDescending(a => a.DataFinalizacao ?? a.DataEnvio);
            }
            else
            {
                query = query.OrderByDescending(a => a.DataEnvio);
            }

            return await query
                .Select(a => new HistoricoAdocaoItem
                {
                    Id = a.Id,
                    PetId = a.PetId,
                    NomePet = a.Pet.Nome,
                    NomeArquivoImagem = a.Pet.NomeArquivoImagem,
                    EspeciePet = a.Pet.Especie.GetEnumMemberValue(),
                    RacaPet = a.Pet.Raca,
                    DataEnvio = a.DataEnvio,
                    DataResposta = a.DataResposta,
                    DataFinalizacao = a.DataFinalizacao,
                    Status = a.Status.ObterValorMembroEnum(),
                    Observacoes = a.ObservacoesCancelamento
                })
                .ToListAsync();
        }
    }

    public class HistoricoAdocaoItem
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string? NomePet { get; set; }
        public string? NomeArquivoImagem { get; set; }
        public string? EspeciePet { get; set; }
        public string? RacaPet { get; set; }
        public DateTime DataEnvio { get; set; }
        public DateTime? DataResposta { get; set; }
        public DateTime? DataFinalizacao { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
    }
}
