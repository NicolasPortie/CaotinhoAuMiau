namespace CaotinhoAuMiau.Services
{
    public class PetServiceResult
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public int PetId { get; set; }
    }
}
