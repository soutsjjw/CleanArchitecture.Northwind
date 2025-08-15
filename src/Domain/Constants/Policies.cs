namespace CleanArchitecture.Northwind.Domain.Constants;

public abstract class Policies
{
    public const string CanPurge = nameof(CanPurge);

    // Customers
    public const string Customers = "Customers::";
    public const string Customers_Read = "Customers:Read:";
    public const string Customers_Create = "Customers:Create:";
    public const string Customers_Update = "Customers:Update:";
    public const string Customers_Delete = "Customers:Delete:";

    // Orders
    public const string Orders = "Orders::";
    public const string Orders_Read = "Orders:Read:";
    public const string Orders_Create = "Orders:Create:";
    public const string Orders_Update = "Orders:Update:";
    public const string Orders_Delete = "Orders:Delete:";

    // Products
    public const string Products = "Products::";
    public const string Products_Read = "Products:Read:";
    public const string Products_Create = "Products:Create:";
    public const string Products_Update = "Products:Update:";
    public const string Products_Delete = "Products:Delete:";

    // Categories
    public const string Categories = "Categories::";
    public const string Categories_Read = "Categories:Read";
    public const string Categories_Create = "Categories:Create:";
    public const string Categories_Update = "Categories:Update:";
    public const string Categories_Delete = "Categories:Delete:";

    // Suppliers
    public const string Suppliers = "Suppliers::";
    public const string Suppliers_Read = "Suppliers:Read";
    public const string Suppliers_Create = "Suppliers:Create:";
    public const string Suppliers_Update = "Suppliers:Update:";
    public const string Suppliers_Delete = "Suppliers:Delete:";

    // Employees
    public const string Employees = "Employees::";
    public const string Employees_Read = "Employees:Read";
    public const string Employees_Create = "Employees:Create:";
    public const string Employees_Update = "Employees:Update:";
    public const string Employees_Delete = "Employees:Delete:";

    // Territories / Regions
    public const string Territories = "Territories::";
    public const string Territories_Read = "Territories:Read";
    public const string Territories_Create = "Territories:Create:";
    public const string Territories_Update = "Territories:Update:";
    public const string Territories_Delete = "Territories:Delete:";

    public const string Regions = "Regions::";
    public const string Regions_Read = "Regions:Read";
    public const string Regions_Create = "Regions:Create:";
    public const string Regions_Update = "Regions:Update:";
    public const string Regions_Delete = "Regions:Delete:";

    // Shippers
    public const string Shippers = "Shippers::";
    public const string Shippers_Read = "Shippers:Read";
    public const string Shippers_Create = "Shippers:Create:";
    public const string Shippers_Update = "Shippers:Update:";
    public const string Shippers_Delete = "Shippers:Delete:";

    // CustDemo
    public const string CustDemo = "CustDemo::";
    public const string CustDemo_Read = "CustDemo:Read";
    public const string CustDemo_Create = "CustDemo:Create:";
    public const string CustDemo_Update = "CustDemo:Update:";
    public const string CustDemo_Delete = "CustDemo:Delete:";

    // EmpTerr
    public const string EmpTerr = "EmpTerr::";
    public const string EmpTerr_Read = "EmpTerr:Read";
    public const string EmpTerr_Create = "EmpTerr:Create:";
    public const string EmpTerr_Update = "EmpTerr:Update:";
    public const string EmpTerr_Delete = "EmpTerr:Delete:";

    // Audit
    public const string Audit_Read = "Audit:Read";
}
