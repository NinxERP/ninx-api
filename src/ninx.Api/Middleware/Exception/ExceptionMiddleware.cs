using System.Net;
using System.Text.Json;
using ninx.Communication.Response;
using ninx.Domain.Exceptions;

namespace ninx.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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
                    _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor.")
                };

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

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}