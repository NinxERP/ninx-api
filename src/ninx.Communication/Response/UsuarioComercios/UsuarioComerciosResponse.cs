namespace ninx.Communication.Response
{
    public class UsuarioComercioResponse
    {
        public int UsuarioComercioID { get; set; }
        public int UsuarioID { get; set; }
        public int ComercioID { get; set; }
        public string Permissao { get; set; } = null!;
        public bool Ativo { get; set; }
    }
}