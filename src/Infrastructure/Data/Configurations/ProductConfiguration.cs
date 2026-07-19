using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        // PK
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
               .HasColumnName("ProductID")
               .ValueGeneratedOnAdd();

        // ProductName
        builder.Property(p => p.ProductName)
               .HasMaxLength(40)
               .IsRequired();

        // Supplier FK
        builder.Property(p => p.SupplierId)
               .HasColumnName("SupplierID")
               .IsRequired(false);
        builder.HasOne(p => p.Supplier)
               .WithMany(s => s.Products)
               .HasForeignKey(p => p.SupplierId);

        // Category FK
        builder.Property(p => p.CategoryId)
               .HasColumnName("CategoryID")
               .IsRequired(false);
        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId);

        // QuantityPerUnit
        builder.Property(p => p.QuantityPerUnit)
               .HasMaxLength(20)
               .IsRequired(false);

        // Pricing & inventory
        builder.Property(p => p.UnitPrice)
               .HasColumnType("money")
               .HasDefaultValue(0m)
               .IsRequired(false);
        builder.Property(p => p.UnitsInStock)
               .HasDefaultValue((short)0)
               .IsRequired(false);
        builder.Property(p => p.UnitsOnOrder)
               .HasDefaultValue((short)0)
               .IsRequired(false);
        builder.Property(p => p.ReorderLevel)
               .HasDefaultValue((short)0)
               .IsRequired(false);

        // Discontinued flag
        builder.Property(p => p.Discontinued)
               .IsRequired();

        // OrderDetails relation
        builder.HasMany(p => p.OrderDetails)
               .WithOne(od => od.Product)
               .HasForeignKey(od => od.ProductId);
    }
}
