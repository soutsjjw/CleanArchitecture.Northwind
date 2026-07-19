using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments"); // 資料表名稱

        builder.HasKey(d => d.DepartmentId);

        builder.Property(d => d.DeptCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(d => d.DeptName)
            .IsRequired()
            .HasMaxLength(50);

        // 一對多關係
        builder.HasMany(d => d.Offices)
            .WithOne(o => o.Department)
            .HasForeignKey(o => o.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
