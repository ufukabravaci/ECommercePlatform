namespace ECommercePlatform.Application.Exceptions;

public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("Bu işlem için yetkiniz bulunmamaktadır.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
