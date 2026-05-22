// src/Infrastructure/Persistence/Configurations/TransactionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        
        builder.HasKey(t => t.Id);
        
        // Convertir CardNumber (Value Object) a string
        builder.Property(t => t.CardNumber)
            .HasConversion(
                v => v.Value,
                v => CardNumber.Create(v).Value
            )
            .HasMaxLength(19)
            .IsRequired();
        
        // Convertir Money (Value Object) a dos columnas: Amount y Currency
        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();
            
            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });
        
        // NOTA: CVV NO se almacena (PCI compliance)
        
        builder.Property(t => t.CreatedAt)
            .IsRequired();
        
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        
        // Índices para búsquedas rápidas
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.CardNumber);
    }
}