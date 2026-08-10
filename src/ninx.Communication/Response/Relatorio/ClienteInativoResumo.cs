namespace ninx.Communication
{
    public class ClienteInativoResumo
    {
        public int ClienteID { get; set; }
        public string ClienteNome { get; set; } = null!;
        public string? Telefone { get; set; }
        public DateTime? UltimaCompra { get; set; }
        public int? DiasSemComprar { get; set; }
        public bool NuncaComprou => UltimaCompra == null;
    }
}
