using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        // 主鍵
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
               .HasColumnName("SupplierID")
               .ValueGeneratedOnAdd();

        // 欄位設定
        builder.Property(s => s.CompanyName)
               .HasColumnName("CompanyName")
               .HasMaxLength(40)
               .IsRequired();

        builder.Property(s => s.ContactName)
               .HasColumnName("ContactName")
               .HasMaxLength(30)
               .IsRequired(false);

        builder.Property(s => s.ContactTitle)
               .HasColumnName("ContactTitle")
               .HasMaxLength(30)
               .IsRequired(false);

        builder.Property(s => s.Address)
               .HasColumnName("Address")
               .HasMaxLength(60)
               .IsRequired(false);

        builder.Property(s => s.City)
               .HasColumnName("City")
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(s => s.Region)
               .HasColumnName("Region")
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(s => s.PostalCode)
               .HasColumnName("PostalCode")
               .HasMaxLength(10)
               .IsRequired(false);

        builder.Property(s => s.Country)
               .HasColumnName("Country")
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(s => s.Phone)
               .HasColumnName("Phone")
               .HasMaxLength(24)
               .IsRequired(false);

        builder.Property(s => s.Fax)
               .HasColumnName("Fax")
               .HasMaxLength(24)
               .IsRequired(false);

        builder.Property(s => s.HomePage)
               .HasColumnName("HomePage")
               .HasColumnType("ntext")
               .IsRequired(false);

        // 關聯設定：Suppliers 1 - * Products
        builder.HasMany(s => s.Products)
               .WithOne(p => p.Supplier)
               .HasForeignKey(p => p.SupplierId);
    }
}
