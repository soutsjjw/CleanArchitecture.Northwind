using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class ShipperConfiguration : IEntityTypeConfiguration<Shipper>
{
    public void Configure(EntityTypeBuilder<Shipper> builder)
    {
        // 對應資料表
        builder.ToTable("Shippers");

        // 主鍵設定
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
               .HasColumnName("ShipperID")
               .ValueGeneratedOnAdd();

        // CompanyName
        builder.Property(s => s.CompanyName)
               .HasColumnName("CompanyName")
               .HasMaxLength(40)
               .IsRequired();

        // Phone
        builder.Property(s => s.Phone)
               .HasColumnName("Phone")
               .HasMaxLength(24)
               .IsRequired(false);

        // 關聯設定：Shipper 1 - * Orders
        builder.HasMany(s => s.Orders)
               .WithOne(o => o.Shipper)
               .HasForeignKey(o => o.ShipVia);
    }
}
