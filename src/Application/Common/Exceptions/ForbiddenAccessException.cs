namespace CleanArchitecture.Northwind.Application.Common.Exceptions;

public class ForbiddenAccessException : CustomException
{
    public ForbiddenAccessException() : base("", null, System.Net.HttpStatusCode.Forbidden) { }

    public ForbiddenAccessException(string message) : base(message, null, System.Net.HttpStatusCode.Forbidden) { }
}
