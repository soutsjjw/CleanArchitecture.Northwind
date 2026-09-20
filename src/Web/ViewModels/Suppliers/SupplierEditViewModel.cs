using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;

namespace CleanArchitecture.Northwind.Web.ViewModels.Suppliers;

public sealed class SupplierEditViewModel
{
    public string? ProtectedId { get; set; }
    public string? Keyword { get; set; }
    public bool? IsActiveFilter { get; set; }
    public SupplierSortField? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    [Required(ErrorMessage = "供應商公司名稱不可為空。")]
    [StringLength(40)] public string CompanyName { get; set; } = string.Empty;
    [StringLength(30)] public string? ContactName { get; set; }
    [StringLength(30)] public string? ContactTitle { get; set; }
    [StringLength(60)] public string? Address { get; set; }
    [StringLength(15)] public string? City { get; set; }
    [StringLength(15)] public string? Region { get; set; }
    [StringLength(10)] public string? PostalCode { get; set; }
    [StringLength(15)] public string? Country { get; set; }
    [StringLength(24)] public string? Phone { get; set; }
    [StringLength(24)] public string? Fax { get; set; }
    public string? HomePage { get; set; }
    public bool IsActive { get; set; } = true;
    public int ProductCount { get; set; }
}
