namespace ninx.Communication
{
    public class VendaDocumentoResumo
    {
        public int VendaID { get; set; }
        public Guid DocumentoGuid { get; set; }
        public bool Assinado { get; set; }
    }
}
