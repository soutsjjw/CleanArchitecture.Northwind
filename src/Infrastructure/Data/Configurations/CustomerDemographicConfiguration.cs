using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class CustomerDemographicConfiguration
    : IEntityTypeConfiguration<CustomerDemographic>
{
    public void Configure(EntityTypeBuilder<CustomerDemographic> builder)
    {
        // 對應到資料表
        builder.ToTable("CustomerDemographics");

        // 主鍵設定
        builder.HasKey(cd => cd.Id);

        builder.Property(cd => cd.Id)
               .HasColumnName("CustomerTypeID")
               .HasColumnType("nchar(10)")
               .IsFixedLength()
               .IsRequired()
               .ValueGeneratedNever();

        // 描述欄位
        builder.Property(cd => cd.CustomerDesc)
               .HasColumnName("CustomerDesc")
               .HasColumnType("ntext")
               .IsRequired(false);

        // 關聯設定：CustomerDemographics 1 - * CustomerCustomerDemo
        builder.HasMany(cd => cd.CustomerCustomerDemos)
               .WithOne(ccd => ccd.CustomerDemographic)
               .HasForeignKey(ccd => ccd.CustomerTypeId);
    }
}
