namespace CleanArchitecture.Northwind.Domain.Constants;

public static class Modules
{
    public const string Customers = nameof(Customers);
    public const string Orders = nameof(Orders);
    public const string Products = nameof(Products);
    public const string Suppliers = nameof(Suppliers);
    public const string Employees = nameof(Employees);

    public static readonly string[] All = { Customers, Orders, Products, Suppliers, Employees };
}
