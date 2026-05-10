using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations;

public class RuleConfiguration : IEntityTypeConfiguration<Rule>
{
    public void Configure(EntityTypeBuilder<Rule> builder)
    {
        builder.ToTable("Rules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasData(
            new { Id = 1, Name = "Non-fumeur" },
            new { Id = 2, Name = "Animaux de compagnie acceptés" },
            new { Id = 3, Name = "Silence requis" },
            new { Id = 4, Name = "Appels téléphoniques interdits" },
            new { Id = 5, Name = "Nourriture autorisée" }
        );
    }
}
