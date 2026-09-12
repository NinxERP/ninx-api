namespace ninx.Domain.Entities
{
    public class Permissao
    {
        public int PermissaoID { get; set; }
        public string Chave { get; set; } = null!;
        public string Nome { get; set; } = null!;
        public string? Descricao { get; set; }

        public ICollection<CargoPermissao> CargoPermissoes { get; set; } = [];
    }
}
