namespace ninx.Domain.Entities
{
    public class CargoPermissao
    {
        public int CargoPermissaoID { get; set; }
        public int CargoID { get; set; }
        public int PermissaoID { get; set; }

        public Cargo Cargo { get; set; } = null!;
        public Permissao Permissao { get; set; } = null!;
    }
}
