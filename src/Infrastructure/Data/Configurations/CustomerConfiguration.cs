using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // 主鍵設定
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
               .HasColumnName("CustomerID")
               .HasColumnType("nchar(5)")
               .IsFixedLength()
               .IsRequired()
               .ValueGeneratedNever();

        // 欄位設定
        builder.Property(c => c.CompanyName)
               .IsRequired()
               .HasMaxLength(40);

        builder.Property(c => c.ContactName)
               .HasMaxLength(30)
               .IsRequired(false);

        builder.Property(c => c.ContactTitle)
               .HasMaxLength(30)
               .IsRequired(false);

        builder.Property(c => c.Address)
               .HasMaxLength(60)
               .IsRequired(false);

        builder.Property(c => c.City)
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(c => c.Region)
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(c => c.PostalCode)
               .HasMaxLength(10)
               .IsRequired(false);

        builder.Property(c => c.Country)
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(c => c.Phone)
               .HasMaxLength(24)
               .IsRequired(false);

        builder.Property(c => c.Fax)
               .HasMaxLength(24)
               .IsRequired(false);

        // 關聯設定：Customer 1 - * Order
        builder.HasMany(c => c.Orders)
               .WithOne(o => o.Customer)
               .HasForeignKey(o => o.CustomerId);
        // Orders.CustomerID 外鍵對應到 Customers.CustomerID :contentReference[oaicite:12]{index=12}
    }
}
