using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Northwind.Web.ViewModels.Products;

public sealed class InventoryAdjustmentViewModel
{
    [Required]
    public string ProtectedId { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public short UnitsInStock { get; set; }

    [Range(short.MinValue, short.MaxValue)]
    public short QuantityDelta { get; set; }

    [Required(ErrorMessage = "異動原因不可為空。")]
    [StringLength(250, ErrorMessage = "異動原因不可超過 250 個字元。")]
    public string Reason { get; set; } = string.Empty;

    [Required]
    public string RowVersion { get; set; } = string.Empty;
}
