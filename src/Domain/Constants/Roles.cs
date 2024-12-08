namespace CleanArchitecture.Northwind.Domain.Constants;

public abstract class Roles
{
    /// <summary>
    /// 系統管理員
    /// </summary>
    public const string Administrator = nameof(Administrator);

    /// <summary>
    /// 銷售代表
    /// </summary>
    public const string Sales = nameof(Sales);

    /// <summary>
    /// 倉庫經理
    /// </summary>
    public const string Warehouse = nameof(Warehouse);

    /// <summary>
    /// 採購代理
    /// </summary>
    public const string Purchase = nameof(Purchase);

    /// <summary>
    /// 財務人員
    /// </summary>
    public const string Finance = nameof(Finance);

    /// <summary>
    /// 客戶服務代表
    /// </summary>
    public const string CustomerService = nameof(CustomerService);
}
