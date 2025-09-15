using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CaotinhoAuMiau.Models;

namespace CaotinhoAuMiau.Services
{
    public interface IPetService
    {
        Task<bool> NomeJaExisteAsync(string nome, int id);
        Task<PetServiceResult> SalvarPetAsync(Pet pet, IFormFile? foto, string webRootPath, bool removerImagem, bool cadastroCompleto, bool manterImagemAtual);
    }
}
