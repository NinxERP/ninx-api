namespace ninx.Communication
{
    public class AtualizarUsuarioComercioRequest
    {
        public int UsuarioID { get; set; }
        public int ComercioID { get; set; }
        public int CargoID { get; set; }
        public bool? Ativo { get; set; }
    }
}
