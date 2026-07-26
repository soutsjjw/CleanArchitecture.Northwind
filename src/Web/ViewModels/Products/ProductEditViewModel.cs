using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Northwind.Web.ViewModels.Products;

public sealed class ProductEditViewModel
{
    public string? ProtectedId { get; set; }

    [Required(ErrorMessage = "商品名稱不可為空。")]
    [StringLength(40, ErrorMessage = "商品名稱不可超過 40 個字元。")]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "請選擇商品分類。")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "請選擇供應商。")]
    public int SupplierId { get; set; }

    [StringLength(20, ErrorMessage = "包裝量不可超過 20 個字元。")]
    public string? QuantityPerUnit { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335",
        ErrorMessage = "商品單價不可小於 0。")]
    public decimal? UnitPrice { get; set; }

    [Range(0, short.MaxValue, ErrorMessage = "再訂購水準不可小於 0。")]
    public short? ReorderLevel { get; set; }

    public IFormFile? Picture { get; set; }

    public bool RemovePicture { get; set; }

    public bool HasPicture { get; set; }

    public IReadOnlyList<ProductOptionViewModel> Categories { get; set; } = [];

    public IReadOnlyList<ProductOptionViewModel> Suppliers { get; set; } = [];
}
