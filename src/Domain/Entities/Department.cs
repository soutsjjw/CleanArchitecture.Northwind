using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Domain.Entities;

public class Department
{
    [Key]
    public int DepartmentId { get; set; }

    [Required, MaxLength(10)]
    public string DeptCode { get; set; } = default!;

    [Required, MaxLength(50)]
    public string DeptName { get; set; } = default!;

    // 導覽屬性 - 一個部門有多個單位
    public ICollection<Office> Offices { get; set; } = new List<Office>();
}
