namespace ninx.Domain.Regras
{
    /// <summary>
    /// Vencimento da venda fiada: o próximo "dia fixo do mês" configurado no comércio, depois da data da venda.
    /// </summary>
    public static class VencimentoFiado
    {
        public const int DiaPadrao = 10;

        /// <remarks>
        /// Uma venda feita no próprio dia de vencimento vence no mês seguinte. Em meses mais curtos
        /// que o dia configurado (ex.: dia 31 em fevereiro), vence no último dia do mês.
        /// </remarks>
        public static DateTime Calcular(DateTime dataVenda, int diaVencimento)
        {
            var noMes = NoMes(dataVenda.Year, dataVenda.Month, diaVencimento);
            if (noMes > dataVenda.Date)
                return noMes;

            var mesSeguinte = dataVenda.AddMonths(1);
            return NoMes(mesSeguinte.Year, mesSeguinte.Month, diaVencimento);
        }

        private static DateTime NoMes(int ano, int mes, int dia) =>
            new(ano, mes, Math.Min(dia, DateTime.DaysInMonth(ano, mes)), 0, 0, 0, DateTimeKind.Utc);
    }
}
