namespace ninx.Communication
{
    public class UsuarioComercioResponse
    {
        public int UsuarioComercioID { get; set; }
        public int UsuarioID { get; set; }
        public int ComercioID { get; set; }
        public int CargoID { get; set; }
        public string CargoNome { get; set; } = null!;
        public int CargoPeso { get; set; }
        public bool Ativo { get; set; }
    }
}