using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class CustomerCustomerDemoConfiguration
    : IEntityTypeConfiguration<CustomerCustomerDemo>
{
    public void Configure(EntityTypeBuilder<CustomerCustomerDemo> builder)
    {
        // 對應資料表
        builder.ToTable("CustomerCustomerDemo");

        // 複合主鍵：CustomerID + CustomerTypeID
        builder.HasKey(ccd => new { ccd.CustomerId, ccd.CustomerTypeId });

        // 欄位屬性
        builder.Property(ccd => ccd.CustomerId)
               .HasColumnName("CustomerID")
               .HasColumnType("nchar(5)")
               .IsFixedLength()
               .IsRequired();

        builder.Property(ccd => ccd.CustomerTypeId)
               .HasColumnName("CustomerTypeID")
               .HasColumnType("nchar(10)")
               .IsFixedLength()
               .IsRequired();

        // 與 Customers 1 : * CustomerCustomerDemo
        builder.HasOne(ccd => ccd.Customer)
               .WithMany(c => c.CustomerCustomerDemos)
               .HasForeignKey(ccd => ccd.CustomerId);

        // 與 CustomerDemographics 1 : * CustomerCustomerDemo
        builder.HasOne(ccd => ccd.CustomerDemographic)
               .WithMany(cd => cd.CustomerCustomerDemos)
               .HasForeignKey(ccd => ccd.CustomerTypeId);
    }
}
