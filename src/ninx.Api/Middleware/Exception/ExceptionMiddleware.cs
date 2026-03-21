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

                if (exception is BadRequestException)
                    await HandleExceptionAsync(context, HttpStatusCode.BadRequest, exception.Message);
                else if (exception is NotFoundException)
                    await HandleExceptionAsync(context, HttpStatusCode.NotFound, exception.Message);
                else if (exception is UnauthorizedException)
                    await HandleExceptionAsync(context, HttpStatusCode.Unauthorized, exception.Message);
                else
                    await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Erro interno do servidor.");
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