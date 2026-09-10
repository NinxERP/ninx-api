namespace ninx.Communication
{
    public class CargoResponse
    {
        public int CargoID { get; set; }
        public string Nome { get; set; } = null!;
        public int Peso { get; set; }
        public int? ComercioID { get; set; }
        public bool Ativo { get; set; }
        public bool Reservado { get; set; }
    }
}
