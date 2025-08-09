using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        // 對應資料表名稱
        builder.ToTable("OrderDetails");

        // 複合主鍵：OrderID + ProductID
        builder.HasKey(od => new { od.OrderId, od.ProductId });

        // 欄位映射
        builder.Property(od => od.OrderId)
               .HasColumnName("OrderID");

        builder.Property(od => od.ProductId)
               .HasColumnName("ProductID");

        builder.Property(od => od.UnitPrice)
               .HasColumnName("UnitPrice")
               .HasColumnType("money")
               .IsRequired();

        builder.Property(od => od.Quantity)
               .HasColumnName("Quantity")
               .IsRequired();

        builder.Property(od => od.Discount)
               .HasColumnName("Discount")
               .IsRequired();

        // 關聯設定：Order 1 - * OrderDetails
        builder.HasOne(od => od.Order)
               .WithMany(o => o.OrderDetails)
               .HasForeignKey(od => od.OrderId);

        // 關聯設定：Product 1 - * OrderDetails
        builder.HasOne(od => od.Product)
               .WithMany(p => p.OrderDetails)
               .HasForeignKey(od => od.ProductId);
    }
}
