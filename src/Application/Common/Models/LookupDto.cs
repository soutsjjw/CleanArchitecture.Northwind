namespace CleanArchitecture.Northwind.Application.Common.Models;

public class LookupDto
{
    public int Id { get; init; }

    public string? Title { get; init; }

    private class Mapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
        }
    }
}
