using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Web.ViewModels.Products;

public sealed class StocktakeViewModel
{
    [Required]
    public string ProtectedId { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public short UnitsInStock { get; set; }

    [Range(0, short.MaxValue, ErrorMessage = "盤點數量不可小於 0。")]
    public short ActualQuantity { get; set; }

    [Required(ErrorMessage = "盤點原因不可為空。")]
    [StringLength(250, ErrorMessage = "盤點原因不可超過 250 個字元。")]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public string RowVersion { get; set; } = string.Empty;
}
