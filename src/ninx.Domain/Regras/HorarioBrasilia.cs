namespace ninx.Domain.Regras
{
    /// <summary>
    /// Horário de Brasília para o que é impresso em documentos e para a data de vencimento.
    /// O banco grava tudo em UTC; quem lê o documento vê o relógio do próprio balcão, e uma venda
    /// às 21h do dia 28 não pode sair datada do dia 29.
    /// </summary>
    /// <remarks>
    /// ponytail: deslocamento fixo de UTC-3, que vale desde o fim do horário de verão em 2019 e
    /// não depende de tzdata no contêiner. Se o produto atender outros fusos (o Amazonas é UTC-4),
    /// trocar por um fuso persistido por comércio.
    /// </remarks>
    public static class HorarioBrasilia
    {
        private static readonly TimeSpan Deslocamento = TimeSpan.FromHours(-3);

        public const string Rotulo = "UTC-03:00";

        /// <summary>Converte um instante em UTC para o relógio de Brasília.</summary>
        public static DateTime Converter(DateTime utc) =>
            DateTime.SpecifyKind(utc, DateTimeKind.Utc).Add(Deslocamento).ToUnspecified();

        /// <summary>"28/09/2026 21:07:13 (UTC-03:00)": data, hora e o deslocamento, sem ambiguidade.</summary>
        public static string Formatar(DateTime utc) =>
            $"{Converter(utc):dd/MM/yyyy HH:mm:ss} ({Rotulo})";

        private static DateTime ToUnspecified(this DateTime valor) =>
            DateTime.SpecifyKind(valor, DateTimeKind.Unspecified);
    }
}
