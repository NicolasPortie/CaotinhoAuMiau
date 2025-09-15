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
            string hashSenhaInformada = GerarHashSenha(senhaInformada);
            
            return CompararHashesDeFormaSegura(hashSenhaInformada, hashArmazenado);
        }
        
        private static bool CompararHashesDeFormaSegura(string hash1, string hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;
                
            int resultado = 0;
            
            for (int i = 0; i < hash1.Length; i++)
            {
                resultado |= hash1[i] ^ hash2[i];
            }
            
            return resultado == 0;
        }
    }
}
