using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class OfficeConfiguration : IEntityTypeConfiguration<Office>
{
    public void Configure(EntityTypeBuilder<Office> builder)
    {
        builder.ToTable("Offices");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OfficeCode)
            .IsRequired()
            .HasMaxLength(15);

        builder.Property(o => o.OfficeName)
            .IsRequired()
            .HasMaxLength(50);
    }
}
