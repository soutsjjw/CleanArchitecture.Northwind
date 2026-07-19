using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        // 對應資料表
        builder.ToTable("Employees");

        // PK
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .HasColumnName("EmployeeID")
               .ValueGeneratedOnAdd();

        // 欄位設定
        builder.Property(e => e.LastName)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(e => e.FirstName)
               .IsRequired()
               .HasMaxLength(10);

        builder.Property(e => e.Title)
               .HasMaxLength(30)
               .IsRequired(false);

        builder.Property(e => e.TitleOfCourtesy)
               .HasMaxLength(25)
               .IsRequired(false);

        builder.Property(e => e.BirthDate)
               .IsRequired(false);

        builder.Property(e => e.HireDate)
               .IsRequired(false);

        builder.Property(e => e.Address)
               .HasMaxLength(60)
               .IsRequired(false);

        builder.Property(e => e.City)
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(e => e.Region)
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(e => e.PostalCode)
               .HasMaxLength(10)
               .IsRequired(false);

        builder.Property(e => e.Country)
               .HasMaxLength(15)
               .IsRequired(false);

        builder.Property(e => e.HomePhone)
               .HasMaxLength(24)
               .IsRequired(false);

        builder.Property(e => e.Extension)
               .HasMaxLength(4)
               .IsRequired(false);

        builder.Property(e => e.Photo)
               .HasColumnType("image")
               .IsRequired(false);

        builder.Property(e => e.Notes)
               .HasColumnType("ntext")
               .IsRequired();

        builder.Property(e => e.PhotoPath)
               .HasMaxLength(255)
               .IsRequired(false);

        // Self-referencing FK: ReportsTo -> EmployeeID
        builder.HasOne(e => e.Manager)
               .WithMany(m => m.DirectReports)
               .HasForeignKey(e => e.ReportsTo)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
