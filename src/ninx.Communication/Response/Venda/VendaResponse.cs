namespace ninx.Communication
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
        public List<DocumentoAssinaturaResponse> Documentos { get; set; } = new List<DocumentoAssinaturaResponse>();
        public decimal ValorPago { get; set; }
        public decimal SaldoDevedor { get; set; }
    }
}
