namespace ninx.Communication.Response
{
    public class VendaResponse
    {
        public int VendaID { get; set; }
        public int ComercioID { get; set; }
        public int UsuarioID { get; set; }
        public decimal Total { get; set; }
        public string? TipoVenda { get; set; }
        public string? Status { get; set; } 
        public DateTime? CriadoEm { get; set; } 
        public Guid DocumentoGuid { get; set; }
    }
}
