using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        // PK
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
               .HasColumnName("OrderID")
               .ValueGeneratedOnAdd();

        // FK: Customer
        builder.Property(o => o.CustomerId)
               .HasColumnName("CustomerID")
               .HasColumnType("nchar(5)")
               .IsFixedLength()
               .IsRequired(false);
        builder.HasOne(o => o.Customer)
               .WithMany(c => c.Orders)
               .HasForeignKey(o => o.CustomerId);

        // FK: Employee
        builder.Property(o => o.EmployeeId)
               .HasColumnName("EmployeeID")
               .IsRequired(false);
        builder.HasOne(o => o.Employee)
               .WithMany(e => e.Orders)
               .HasForeignKey(o => o.EmployeeId);

        // Dates
        builder.Property(o => o.OrderDate)
               .IsRequired(false);
        builder.Property(o => o.RequiredDate)
               .IsRequired(false);
        builder.Property(o => o.ShippedDate)
               .IsRequired(false);

        // FK: Shipper
        builder.Property(o => o.ShipVia)
               .HasColumnName("ShipVia")
               .IsRequired(false);
        builder.HasOne(o => o.Shipper)
               .WithMany(s => s.Orders)
               .HasForeignKey(o => o.ShipVia);

        // Freight
        builder.Property(o => o.Freight)
               .HasColumnName("Freight")
               .HasColumnType("money")
               .HasDefaultValue(0m)
               .IsRequired(false);

        // Shipping info
        builder.Property(o => o.ShipName)
               .HasMaxLength(40)
               .IsRequired(false);
        builder.Property(o => o.ShipAddress)
               .HasMaxLength(60)
               .IsRequired(false);
        builder.Property(o => o.ShipCity)
               .HasMaxLength(15)
               .IsRequired(false);
        builder.Property(o => o.ShipRegion)
               .HasMaxLength(15)
               .IsRequired(false);
        builder.Property(o => o.ShipPostalCode)
               .HasMaxLength(10)
               .IsRequired(false);
        builder.Property(o => o.ShipCountry)
               .HasMaxLength(15)
               .IsRequired(false);

        // OrderDetails relation
        builder.HasMany(o => o.OrderDetails)
               .WithOne(od => od.Order)
               .HasForeignKey(od => od.OrderId);
    }
}
