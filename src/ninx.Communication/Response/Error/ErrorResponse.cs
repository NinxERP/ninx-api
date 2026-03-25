namespace ninx.Communication.Response
{
    public class ErrorResponse
    {
        public int Status { get; set; }
        public string Messagem { get; set; } = null!;
    }
}