namespace ninx.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}

// ninx.Domain/Exceptions/BadRequestException.cs
namespace ninx.Domain.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }
}

// ninx.Domain/Exceptions/UnauthorizedException.cs
namespace ninx.Domain.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}