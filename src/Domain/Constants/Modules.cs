namespace CleanArchitecture.Northwind.Domain.Constants;

public static class Modules
{
    public const string Customers = "Customers";
    public const string SalesOrders = "SalesOrders";
    public const string Products = "Products";
    public const string Suppliers = "Suppliers";
    public const string Employees = "Employees";

    public static readonly string[] All = { Customers, SalesOrders, Products, Suppliers, Employees };
}
