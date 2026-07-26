namespace CleanArchitecture.Northwind.Domain.Constants;

public static class Modules
{
    public const string Customers = nameof(Customers);
    public const string Orders = nameof(Orders);
    public const string Products = nameof(Products);
    public const string Categories = nameof(Categories);
    public const string Suppliers = nameof(Suppliers);
    public const string Inventory = nameof(Inventory);
    public const string Employees = nameof(Employees);

    public static readonly string[] All =
    [
        Customers,
        Orders,
        Products,
        Categories,
        Suppliers,
        Inventory,
        Employees
    ];
}
