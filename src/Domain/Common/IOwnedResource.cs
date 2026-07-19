namespace CleanArchitecture.Northwind.Domain.Common;

public interface IOwnedResource
{
    string CreatedBy { get; }
    int OfficeId { get; }
    int DepartmentId { get; }
}
