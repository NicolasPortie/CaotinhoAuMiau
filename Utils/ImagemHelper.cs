using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CaotinhoAuMiau.Utils
{
    public static class ImagemHelper
    {
        private const int MaxFileSizeInBytes = 2 * 1024 * 1024; // 2 MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public static async Task<string?> SalvarAsync(IFormFile imagem, string webRootPath, string subpasta, string? nomeAtual = null)
        {
            if (imagem == null || imagem.Length <= 0)
                return null;

            if (imagem.Length > MaxFileSizeInBytes)
            {
                throw new ArgumentException("O arquivo excede o tamanho máximo permitido de 2 MB.");
            }

            var extensao = Path.GetExtension(imagem.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extensao) || !AllowedExtensions.Contains(extensao))
            {
                throw new ArgumentException("Formato de arquivo inválido. Apenas imagens .jpg e .png são permitidas.");
            }

            if (!string.IsNullOrEmpty(nomeAtual))
            {
                Remover(webRootPath, subpasta, nomeAtual);
            }

            var caminhoUpload = Path.Combine(webRootPath, "imagens", subpasta);
            if (!Directory.Exists(caminhoUpload))
            {
                Directory.CreateDirectory(caminhoUpload);
            }

            var nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(imagem.FileName);
            var caminhoArquivo = Path.Combine(caminhoUpload, nomeArquivo);

            using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
            {
                await imagem.CopyToAsync(stream);
            }

            return nomeArquivo;
        }

        public static void Remover(string webRootPath, string subpasta, string nomeArquivo)
        {
            if (string.IsNullOrEmpty(nomeArquivo))
                return;

            var caminho = Path.Combine(webRootPath, "imagens", subpasta, nomeArquivo);
            if (File.Exists(caminho))
            {
                try
                {
                    File.Delete(caminho);
                }
                catch
                {
                }
            }
        }
    }
}
