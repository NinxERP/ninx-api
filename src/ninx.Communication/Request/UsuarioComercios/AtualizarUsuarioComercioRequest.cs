namespace ninx.Communication.Request
{
    public class AtualizarUsuarioComercioRequest
    {
        public int UsuarioID { get; set; }
        public int ComercioID { get; set; }
        public int Permissao { get; set; }
        public bool? Ativo { get; set; }
    }
}
