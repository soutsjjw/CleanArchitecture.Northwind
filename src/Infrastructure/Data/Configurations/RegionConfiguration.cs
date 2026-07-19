using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        // 對應資料表
        builder.ToTable("Region");

        // 主鍵
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
               .HasColumnName("RegionID")
               .IsRequired()
               .ValueGeneratedNever();

        // 欄位：RegionDescription
        builder.Property(r => r.RegionDescription)
               .HasColumnName("RegionDescription")
               .HasColumnType("nchar(50)")
               .IsRequired();

        // 關聯：Region 1 - * Territories
        builder.HasMany(r => r.Territories)
               .WithOne(t => t.Region)
               .HasForeignKey(t => t.RegionId);
    }
}
