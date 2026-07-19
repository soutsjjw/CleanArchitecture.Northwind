using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Each User can have many UserClaims
        builder.HasMany(e => e.Claims)
            .WithOne()
            .HasForeignKey(uc => uc.UserId)
            .IsRequired();

        // Each User can have many UserLogins
        builder.HasMany(e => e.Logins)
            .WithOne()
            .HasForeignKey(ul => ul.UserId)
            .IsRequired();

        // Each User can have many UserTokens
        builder.HasMany(e => e.Tokens)
            .WithOne()
            .HasForeignKey(ut => ut.UserId)
            .IsRequired();

        // Each User can have many entries in the UserRole join table
        builder.HasMany(e => e.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<ApplicationUserProfile>(p => p.UserId);
    }
}

public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
        builder.HasOne(d => d.Role)
              .WithMany(p => p.RoleClaims)
              .HasForeignKey(d => d.RoleId)
              .OnDelete(DeleteBehavior.Cascade);

        // Identity 預設表：AspNetRoleClaims
        builder.ToTable("AspNetRoleClaims");

        // 欄位對應與限制
        builder.Property(rc => rc.RoleClaimDescription)
               .HasColumnName("Description")   // 需求：改名
               .HasMaxLength(256)              // 建議：限制長度，避免無限 nvarchar
               .IsUnicode(true);               // 依你情境可改為 IsUnicode(false)

        builder.Property(rc => rc.RoleClaimGroup)
               .HasColumnName("Group")         // 需求：改名（關鍵字交由 EF 自動加方括號）
               .HasMaxLength(64)               // 建議：群組名稱通常較短
               .IsUnicode(true);

        // 常用索引（查角色＋群組）
        builder.HasIndex(rc => new { rc.RoleId, rc.RoleClaimGroup })
               .HasDatabaseName("IX_RoleClaims_Role_Group");
    }
}

public class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.HasOne(d => d.Role)
              .WithMany(p => p.UserRoles)
              .HasForeignKey(d => d.RoleId)
              .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.User)
              .WithMany(p => p.UserRoles)
              .HasForeignKey(d => d.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
    {
        // 此設置沒有作用，必須在 ApplicationDbContext -> OnModelCreating 再進行設置
        builder.HasOne(uc => uc.User)
            .WithMany(u => u.Claims)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApplicationUserLoginConfiguration : IEntityTypeConfiguration<ApplicationUserLogin>
{
    public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
    {
        builder.HasOne(d => d.User)
              .WithMany(p => p.Logins)
              .HasForeignKey(d => d.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<ApplicationUserToken>
{
    public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
    {
        builder.HasOne(d => d.User)
              .WithMany(p => p.Tokens)
              .HasForeignKey(d => d.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApplicationUserProfileConfiguration : IEntityTypeConfiguration<ApplicationUserProfile>
{
    public void Configure(EntityTypeBuilder<ApplicationUserProfile> builder)
    {
        builder.ToTable("AspNetUserProfiles");
    }
}

public class ApplicationUserPasswordHistoryConfiguration : IEntityTypeConfiguration<ApplicationUserPasswordHistory>
{
    public void Configure(EntityTypeBuilder<ApplicationUserPasswordHistory> builder)
    {
        builder.ToTable("AspNetUserPasswordHistories");
    }
}

