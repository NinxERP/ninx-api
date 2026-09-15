namespace ninx.Application.Validators
{
    /// <summary>Validação de CPF pelos dígitos verificadores. Aceita valor com ou sem máscara.</summary>
    public static class Cpf
    {
        public static bool Valido(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return false;
            var cpf = new string(valor.Where(char.IsDigit).ToArray());
            if (cpf.Length != 11 || cpf.Distinct().Count() == 1) return false;

            int Digito(int tamanho)
            {
                var soma = 0;
                for (var i = 0; i < tamanho; i++) soma += (cpf[i] - '0') * (tamanho + 1 - i);
                var resto = soma % 11;
                return resto < 2 ? 0 : 11 - resto;
            }

            return Digito(9) == cpf[9] - '0' && Digito(10) == cpf[10] - '0';
        }
    }
}
