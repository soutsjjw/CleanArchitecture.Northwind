namespace CleanArchitecture.Northwind.Application.Common.Exceptions;

public class BadRequestException : CustomException
{
    public BadRequestException(string message) : base(message, null, System.Net.HttpStatusCode.BadRequest) { }

    public BadRequestException(List<string> errors) : base("", errors, System.Net.HttpStatusCode.BadRequest) { }
}
