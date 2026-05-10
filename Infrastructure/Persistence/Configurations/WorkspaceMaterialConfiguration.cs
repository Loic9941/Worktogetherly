using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations;

public class WorkspaceMaterialConfiguration : IEntityTypeConfiguration<WorkspaceMaterial>
{
    public void Configure(EntityTypeBuilder<WorkspaceMaterial> builder)
    {
        builder.ToTable("WorkspaceMaterials");
        builder.HasKey(wm => new { wm.WorkspaceId, wm.MaterialId });

        builder.Property(wm => wm.Quantity).IsRequired();

        builder.HasOne(wm => wm.Workspace)
               .WithMany(w => w.WorkspaceMaterials)
               .HasForeignKey(wm => wm.WorkspaceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wm => wm.Material)
               .WithMany(m => m.WorkspaceMaterials)
               .HasForeignKey(wm => wm.MaterialId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
