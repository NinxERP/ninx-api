namespace ninx.Application.Validators
{
    /// <summary>
    /// Validação de CNPJ compartilhada pelos validadores de comércio. Antes havia duas cópias
    /// que conferiam apenas se o valor tinha 14 dígitos, sem calcular os dígitos verificadores.
    /// </summary>
    /// <remarks>
    /// Desde julho de 2026 a Receita Federal emite CNPJ alfanumérico (IN RFB nº 2.229/2024):
    /// as 12 primeiras posições aceitam letras maiúsculas e números, e os 2 dígitos
    /// verificadores continuam numéricos. Cada caractere vale (código ASCII − 48), com pesos
    /// de 2 a 9 da direita para a esquerda e módulo 11. Para um dígito, (ASCII − 48) é o
    /// próprio dígito, então o mesmo cálculo valida os CNPJs numéricos antigos.
    /// </remarks>
    public static class Cnpj
    {
        public static bool Valido(string? cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            var valor = cnpj.Replace(".", "").Replace("/", "").Replace("-", "").ToUpperInvariant();

            if (valor.Length != 14)
                return false;

            for (var i = 0; i < 12; i++)
                if (!char.IsAsciiDigit(valor[i]) && !char.IsAsciiLetterUpper(valor[i]))
                    return false;

            if (!char.IsAsciiDigit(valor[12]) || !char.IsAsciiDigit(valor[13]))
                return false;

            if (valor.All(c => c == valor[0]))
                return false;

            var digito1 = CalcularDigito(valor[..12]);
            var digito2 = CalcularDigito(valor[..12] + digito1);

            return valor[12] - '0' == digito1 && valor[13] - '0' == digito2;
        }

        private static int CalcularDigito(string baseCnpj)
        {
            var soma = 0;
            var peso = 2;
            for (var i = baseCnpj.Length - 1; i >= 0; i--)
            {
                soma += (baseCnpj[i] - 48) * peso;
                peso = peso == 9 ? 2 : peso + 1;
            }

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }
    }
}
