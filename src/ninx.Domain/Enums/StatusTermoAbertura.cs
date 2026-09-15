namespace ninx.Domain.Enums
{
    public enum StatusTermoAbertura
    {
        /// <summary>Gerado, esperando a assinatura do titular.</summary>
        Aguardando = 1,

        /// <summary>Assinado; é o termo vigente da conta.</summary>
        Ativo = 2,

        /// <summary>Era o vigente e foi trocado por um termo mais novo, já assinado.</summary>
        Substituido = 3,

        /// <summary>Nunca foi assinado e perdeu a validade porque outro termo foi gerado.</summary>
        Cancelado = 4
    }
}
