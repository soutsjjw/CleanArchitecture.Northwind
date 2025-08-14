namespace CleanArchitecture.Northwind.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string OfficeId { get; }
    string DepartmentId { get; }
    string DisplayName { get; }
    bool IsInRole(params string[] roleName);
    IEnumerable<string> GetRoles();
}
