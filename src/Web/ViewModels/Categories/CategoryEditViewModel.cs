using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Web.ViewModels.Categories;

public sealed class CategoryEditViewModel
{
    public string? ProtectedId { get; set; }

    [Required(ErrorMessage = "分類名稱不可為空。")]
    [StringLength(15, ErrorMessage = "分類名稱不可超過 15 個字元。")]
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int ProductCount { get; set; }
}
