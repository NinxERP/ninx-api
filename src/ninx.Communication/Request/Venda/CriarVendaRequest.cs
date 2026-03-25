namespace ninx.Communication.Request
{
    public class CriarVendaRequest
    {
            public int ComercioID { get; set; }
            public int UsuarioID { get; set; }
            public int? ClienteID { get; set; }
            public string? Observacoes { get; set; }
            public List<ItemVendaRequest> ItensVenda { get; set; } = new();
    }
}
