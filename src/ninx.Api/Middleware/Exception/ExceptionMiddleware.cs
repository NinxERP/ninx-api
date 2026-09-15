using System.Net;
using System.Text.Json;
using ninx.Communication;
using ninx.Domain.Exceptions;

namespace ninx.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var exception = ex.InnerException ?? ex;

                var (statusCode, message) = exception switch
                {
                    BadRequestException => (HttpStatusCode.BadRequest, exception.Message),
                    NotFoundException => (HttpStatusCode.NotFound, exception.Message),
                    UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message),
                    ConcurrencyException => (HttpStatusCode.Conflict, exception.Message),
                    ForbiddenException => (HttpStatusCode.Forbidden, exception.Message),
                    Microsoft.AspNetCore.Http.BadHttpRequestException { StatusCode: 413 } =>
                        (HttpStatusCode.RequestEntityTooLarge, "Arquivo muito grande. Reduza a assinatura ou tente novamente."),
                    _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor.")
                };

                // Exceções de domínio são fluxo esperado (e já aparecem na linha de log da
                // requisição). Só o erro inesperado precisa da pilha completa no log, já que
                // o cliente recebe apenas uma mensagem genérica.
                if (statusCode == HttpStatusCode.InternalServerError)
                    _logger.LogError(ex, "Erro não tratado em {Metodo} {Rota}. Correlação {CorrelacaoId}.",
                        context.Request.Method, context.Request.Path.Value, context.TraceIdentifier);

                await HandleExceptionAsync(context, statusCode, message);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                Status = (int)statusCode,
                Messagem = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
        }
    }
}