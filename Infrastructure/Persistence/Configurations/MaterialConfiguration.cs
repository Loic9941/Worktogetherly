using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(m => m.Name).IsUnique();

        builder.HasData(
            new { Id = 1, Name = "Écran" },
            new { Id = 2, Name = "Caméra web" },
            new { Id = 3, Name = "Micro" },
            new { Id = 4, Name = "Tableau blanc" },
            new { Id = 5, Name = "Projecteur" },
            new { Id = 6, Name = "Imprimante" },
            new { Id = 7, Name = "Casque audio" }
        );
    }
}
