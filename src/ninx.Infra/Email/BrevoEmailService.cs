using Microsoft.Extensions.Configuration;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using System.Net.Http.Json;

namespace ninx.Infra
{
    public class BrevoEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BrevoEmailService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task EnviarAsync(string destinatarioEmail, string destinatarioNome, string assunto, string htmlContent)
        {
            var payload = new
            {
                sender = new
                {
                    name = _configuration["Brevo:SenderName"],
                    email = _configuration["Brevo:SenderEmail"]
                },
                to = new[]
                {
                    new { email = destinatarioEmail, name = destinatarioNome }
                },
                subject = assunto,
                htmlContent = htmlContent
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("api-key", _configuration["Brevo:ApiKey"]);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new BadRequestException("Falha ao enviar e-mail de redefinição de senha.");
        }
    }
}
