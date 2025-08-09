using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class TerritoryConfiguration : IEntityTypeConfiguration<Territory>
{
    public void Configure(EntityTypeBuilder<Territory> builder)
    {
        // 對應到資料表
        builder.ToTable("Territories");

        // 主鍵設定
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
               .HasColumnName("TerritoryID")
               .HasColumnType("nvarchar(20)")
               .IsRequired()
               .ValueGeneratedNever();

        // 欄位設定
        builder.Property(t => t.TerritoryDescription)
               .HasColumnName("TerritoryDescription")
               .HasColumnType("nchar(50)")
               .IsRequired();

        builder.Property(t => t.RegionId)
               .HasColumnName("RegionID")
               .IsRequired();

        // 關聯設定：Region 1 - * Territories
        builder.HasOne(t => t.Region)
               .WithMany(r => r.Territories)
               .HasForeignKey(t => t.RegionId);
    }
}
