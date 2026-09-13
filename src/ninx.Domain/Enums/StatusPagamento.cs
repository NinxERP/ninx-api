namespace ninx.Domain.Enums
{
    public enum StatusPagamento
    {
        Pago = 1,
        Estornado = 2,

        /// <summary>Registrado, mas o recibo (ou o termo, no caso da entrada) ainda não foi assinado. Não conta no saldo.</summary>
        Pendente = 3,

        /// <summary>Era pendente e a venda foi estornada antes da assinatura. Nunca contou no saldo.</summary>
        Cancelado = 4
    }
}
