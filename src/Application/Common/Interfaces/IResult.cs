namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface IResult
{
    string[] Errors { get; set; }
    string[] Messages { get; set; }
    bool Succeeded { get; set; }
    int StatusCode { get; set; }
}

public interface IResult<out T> : IResult
{
    T Data { get; }
}
