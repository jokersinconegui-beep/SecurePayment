// src/Infrastructure/Persistence/Configurations/MerchantConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistence.Configurations;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("Merchants");
        
        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.MerchantId)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.HasIndex(m => m.MerchantId)
            .IsUnique();
        
        builder.Property(m => m.Email)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.HasIndex(m => m.Email)
            .IsUnique();
        
        builder.Property(m => m.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(m => m.PasswordHash)
            .IsRequired();
        
        builder.Property(m => m.ApiKey)
            .HasMaxLength(64)
            .IsRequired();
        
        builder.HasIndex(m => m.ApiKey)
            .IsUnique();
        
        builder.Property(m => m.IsActive)
            .IsRequired();
        
        builder.Property(m => m.CreatedAt)
            .IsRequired();
    }
}