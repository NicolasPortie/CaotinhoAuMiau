using System.Text.RegularExpressions;

namespace CaotinhoAuMiau.Utils
{
    public static class Validators
    {
        private static readonly Regex CPF_REGEX = new(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$|^\d{11}$", RegexOptions.Compiled);
        private static readonly Regex EMAIL_REGEX = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex TELEFONE_REGEX = new(@"^\(\d{2}\)\s\d{4,5}-\d{4}$|^\d{10,11}$", RegexOptions.Compiled);
        private static readonly Regex CEP_REGEX = new(@"^\d{5}-\d{3}$|^\d{8}$", RegexOptions.Compiled);

        public static bool ValidarCPF(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
                return false;

            if (cpf.All(c => c == cpf[0]))
                return false;

            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf += digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }

        public static bool ValidarEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            return EMAIL_REGEX.IsMatch(email);
        }

        public static bool ValidarTelefone(string telefone)
        {
            if (string.IsNullOrEmpty(telefone))
                return false;

            return TELEFONE_REGEX.IsMatch(telefone);
        }

        public static bool ValidarCEP(string cep)
        {
            if (string.IsNullOrEmpty(cep))
                return false;

            return CEP_REGEX.IsMatch(cep);
        }

        public static bool ValidarIdade(DateTime dataNascimento, int idadeMinima = 18)
        {
            var idade = DateTime.Today.Year - dataNascimento.Year;
            if (dataNascimento.Date > DateTime.Today.AddYears(-idade))
                idade--;

            return idade >= idadeMinima;
        }

        public static string FormatarCPF(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return string.Empty;

            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length == 11)
                return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";

            return cpf;
        }

        public static string FormatarTelefone(string telefone)
        {
            if (string.IsNullOrEmpty(telefone))
                return string.Empty;

            telefone = telefone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
            
            if (telefone.Length == 11)
                return $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 5)}-{telefone.Substring(7, 4)}";
            else if (telefone.Length == 10)
                return $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 4)}-{telefone.Substring(6, 4)}";

            return telefone;
        }
    }
}