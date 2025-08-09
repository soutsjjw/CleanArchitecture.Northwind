using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class EmployeeTerritoryConfiguration
    : IEntityTypeConfiguration<EmployeeTerritory>
{
    public void Configure(EntityTypeBuilder<EmployeeTerritory> builder)
    {
        // 對應資料表
        builder.ToTable("EmployeeTerritories");

        // 複合主鍵：EmployeeID + TerritoryID
        builder.HasKey(et => new { et.EmployeeId, et.TerritoryId });

        // 欄位映射
        builder.Property(et => et.EmployeeId)
               .HasColumnName("EmployeeID")
               .IsRequired();

        builder.Property(et => et.TerritoryId)
               .HasColumnName("TerritoryID")
               .HasColumnType("nvarchar(20)")
               .IsRequired();

        // 關聯設定：Employee 1 - * EmployeeTerritories
        builder.HasOne(et => et.Employee)
               .WithMany(e => e.EmployeeTerritories)
               .HasForeignKey(et => et.EmployeeId);

        // 關聯設定：Territory 1 - * EmployeeTerritories
        builder.HasOne(et => et.Territory)
               .WithMany(t => t.EmployeeTerritories)
               .HasForeignKey(et => et.TerritoryId);
    }
}
