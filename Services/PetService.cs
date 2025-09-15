using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CaotinhoAuMiau.Data;
using CaotinhoAuMiau.Models;
using CaotinhoAuMiau.Models.Enums;
using CaotinhoAuMiau.Utils;

namespace CaotinhoAuMiau.Services
{
    public class PetService : IPetService
    {
        private readonly ApplicationDbContext _contexto;

        public PetService(ApplicationDbContext contexto)
        {
            _contexto = contexto ?? throw new ArgumentNullException(nameof(contexto));
        }

        public async Task<bool> NomeJaExisteAsync(string nome, int id)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return false;

            var nomeNormalizado = nome.Trim();

            return await _contexto.Pets
                .AnyAsync(p =>
                    p.Id != id
                    && p.Status != StatusPet.Adotado
                    && p.Nome.Trim().ToLower() == nomeNormalizado.ToLower());
        }

        public async Task<PetServiceResult> SalvarPetAsync(Pet pet, IFormFile? foto, string webRootPath, bool removerImagem, bool cadastroCompleto, bool manterImagemAtual)
        {
            bool novoPet = pet.Id == 0;

            if (!string.IsNullOrWhiteSpace(pet.Nome))
            {
                bool nomeDuplicado = await NomeJaExisteAsync(pet.Nome, pet.Id);
                if (nomeDuplicado)
                {
                    return new PetServiceResult
                    {
                        Sucesso = false,
                        Mensagem = $"Já existe um pet ativo com o nome '{pet.Nome}'."
                    };
                }
            }

            if (novoPet)
            {
                var agora = DateTime.Now;
                pet.DataCriacao = agora;
                pet.DataAtualizacao = agora;
                pet.Status = cadastroCompleto ? StatusPet.Disponivel : StatusPet.Rascunho;
                pet.CadastroCompleto = cadastroCompleto;
                if (foto != null && foto.Length > 0)
                {
                    pet.NomeArquivoImagem = await ImagemHelper.SalvarAsync(foto, webRootPath, "pets");
                }
                _contexto.Pets.Add(pet);
            }
            else
            {
                var petExistente = await _contexto.Pets.FindAsync(pet.Id);
                if (petExistente == null)
                {
                    return new PetServiceResult { Sucesso = false, Mensagem = "Pet não encontrado" };
                }

                if (petExistente.Status == StatusPet.EmProcesso || petExistente.Status == StatusPet.Adotado)
                {
                    return new PetServiceResult { Sucesso = false, Mensagem = $"Não é possível editar um pet com status '{petExistente.Status}'." };
                }

                petExistente.Nome = pet.Nome?.Trim() ?? string.Empty;
                petExistente.Status = cadastroCompleto ? StatusPet.Disponivel : StatusPet.Rascunho;
                petExistente.CadastroCompleto = cadastroCompleto;
                petExistente.DataAtualizacao = DateTime.Now;
                petExistente.Especie = pet.Especie;
                petExistente.Raca = pet.Raca?.Trim() ?? string.Empty;
                petExistente.Sexo = pet.Sexo;
                petExistente.Porte = pet.Porte?.Trim() ?? string.Empty;
                petExistente.Anos = pet.Anos;
                petExistente.Meses = pet.Meses;
                petExistente.Descricao = pet.Descricao?.Trim() ?? string.Empty;

                if (foto != null && foto.Length > 0)
                {
                    var nomeImg = await ImagemHelper.SalvarAsync(foto, webRootPath, "pets", petExistente.NomeArquivoImagem);
                    if (nomeImg != null)
                        petExistente.NomeArquivoImagem = nomeImg;
                }
                else if (removerImagem && !string.IsNullOrEmpty(petExistente.NomeArquivoImagem))
                {
                    ImagemHelper.Remover(webRootPath, "pets", petExistente.NomeArquivoImagem);
                    petExistente.NomeArquivoImagem = null;
                }

                _contexto.Pets.Update(petExistente);
            }

            await _contexto.SaveChangesAsync();
            return new PetServiceResult { Sucesso = true, Mensagem = novoPet ? "Pet cadastrado com sucesso!" : "Pet atualizado com sucesso!", PetId = pet.Id };
        }
    }
}
