namespace CleanArchitecture.Northwind.Domain.Constants;


public static class AppClaimTypes
{
    public const string Permission = "permission";

    // 使用者身分（供資源範圍比對）
    public const string UserId = System.Security.Claims.ClaimTypes.NameIdentifier;
    public const string OfficeId = "officeid";
    public const string DepartmentId = "departmentid";
}
