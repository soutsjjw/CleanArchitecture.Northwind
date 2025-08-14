using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArchitecture.Northwind.Domain.Entities;

public class Office
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int OfficeId { get; set; }

    public int DepartmentId { get; set; }

    [Required, MaxLength(15)]
    public string OfficeCode { get; set; } = default!;

    [Required, MaxLength(50)]
    public string OfficeName { get; set; } = default!;

    // 導覽屬性 - 每個單位屬於一個部門
    public Department Department { get; set; } = default!;
}
