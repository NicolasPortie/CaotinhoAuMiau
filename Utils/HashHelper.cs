using System;
using System.Security.Cryptography;
using System.Text;

namespace CaotinhoAuMiau.Utils
{
    public static class HashHelper
    {
        public static string GerarHashSenha(string senha)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }
        
        public static bool VerificarSenha(string senhaInformada, string hashArmazenado)
        {
            // Gera o hash da senha informada
            string hashSenhaInformada = GerarHashSenha(senhaInformada);
            
            // Compara os hashes de forma segura
            return CompararHashesDeFormaSegura(hashSenhaInformada, hashArmazenado);
        }
        
        // Compara hashes de forma segura para evitar ataques de timing
        private static bool CompararHashesDeFormaSegura(string hash1, string hash2)
        {
            // Se os comprimentos forem diferentes, retorna falso imediatamente
            if (hash1.Length != hash2.Length)
                return false;
                
            // Usa um método de comparação resistente a ataques de timing
            int resultado = 0;
            
            // Compara cada caractere da string
            for (int i = 0; i < hash1.Length; i++)
            {
                resultado |= hash1[i] ^ hash2[i]; // XOR bit a bit
            }
            
            // Se resultado for 0, as strings são idênticas
            return resultado == 0;
        }
    }
}
