using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ninx.Api.Middlewares
{
    /// <summary>
    /// Registra uma linha por chamada à API: método, rota, status, duração, usuário e
    /// comércio do token e um identificador de correlação devolvido no cabeçalho
    /// <c>X-Correlation-ID</c>.
    /// </summary>
    /// <remarks>
    /// Nunca registra corpo, cabeçalhos ou query string: a assinatura do cliente trafega em
    /// base64 no corpo, e dados pessoais trafegam em várias requisições. A rota é registrada
    /// com os GUIDs mascarados porque, no fluxo de assinatura, o GUID do documento é a
    /// própria credencial do signatário — um log com GUIDs seria uma lista de links válidos.
    /// </remarks>
    public partial class RequestLoggingMiddleware
    {
        private const string HeaderCorrelacao = "X-Correlation-ID";

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Aceita um identificador vindo do cliente apenas se for curto e alfanumérico,
            // para não abrir espaço a injeção de conteúdo no log.
            var correlacao = context.Request.Headers[HeaderCorrelacao].FirstOrDefault();
            if (correlacao is null || !CorrelacaoValida().IsMatch(correlacao))
                correlacao = Guid.NewGuid().ToString("N");

            context.TraceIdentifier = correlacao;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderCorrelacao] = correlacao;
                return Task.CompletedTask;
            });

            var inicio = Stopwatch.GetTimestamp();
            try
            {
                await _next(context);
            }
            finally
            {
                var duracaoMs = Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
                var status = context.Response.StatusCode;

                // 4xx fica em Information: "documento ainda não assinado" responde 400 e é
                // consultado repetidamente pelo PDV; tratá-lo como alerta só geraria ruído.
                var nivel = status >= 500 ? LogLevel.Error : LogLevel.Information;

                _logger.Log(
                    nivel,
                    "HTTP {Metodo} {Rota} respondeu {Status} em {DuracaoMs:0} ms | usuario={UsuarioId} comercio={ComercioId} correlacao={CorrelacaoId}",
                    context.Request.Method,
                    Guids().Replace(context.Request.Path.Value ?? string.Empty, "{guid}"),
                    status,
                    duracaoMs,
                    context.User.FindFirst("usuarioId")?.Value ?? "-",
                    context.User.FindFirst("comercioId")?.Value ?? "-",
                    correlacao);
            }
        }

        [GeneratedRegex("^[A-Za-z0-9-]{1,64}$")]
        private static partial Regex CorrelacaoValida();

        [GeneratedRegex("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")]
        private static partial Regex Guids();
    }
}
