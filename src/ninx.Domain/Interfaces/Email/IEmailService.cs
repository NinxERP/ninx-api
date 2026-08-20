namespace ninx.Domain.Interfaces
{
    public interface IEmailService
    {
        Task EnviarAsync(string destinatarioEmail, string destinatarioNome, string assunto, string htmlContent);
    }
}
