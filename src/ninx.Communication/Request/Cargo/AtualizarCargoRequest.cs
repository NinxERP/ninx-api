namespace ninx.Communication
{
    public class AtualizarCargoRequest
    {
        public string Nome { get; set; } = null!;
        public List<int> PermissaoIds { get; set; } = [];
    }
}
