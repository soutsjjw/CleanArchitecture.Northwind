using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // 1. PK 已在屬性上標註，但這裡再明確一次
        builder.HasKey(c => c.Id);

        // 2. 對應到資料庫欄位
        builder.Property(c => c.Id)
               .HasColumnName("CategoryID");

        // 3. 其他欄位設定
        builder.Property(c => c.CategoryName)
               .IsRequired()
               .HasMaxLength(15);

        builder.Property(c => c.Description)
               .IsRequired(false);

        builder.Property(c => c.Picture)
               .IsRequired(false);

        // 4. 關聯設定：Categories 1 - * Products
        builder.HasMany(c => c.Products)
               .WithOne(p => p.Category)
               .HasForeignKey(p => p.CategoryId);
    }
}
