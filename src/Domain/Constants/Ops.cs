namespace CleanArchitecture.Northwind.Domain.Constants;

public static class Ops
{
    public const string Create = "Create";
    public const string Read = "Read";
    public const string Update = "Update";
    public const string Delete = "Delete";

    public static readonly string[] All = new[] { Create, Read, Update, Delete };
}
