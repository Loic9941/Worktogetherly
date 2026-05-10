using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations
{
    public class SlotConfiguration : IEntityTypeConfiguration<Slot>
    {
        public void Configure(EntityTypeBuilder<Slot> builder)
        {
            builder.ToTable("Slots");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.StartDateTime).IsRequired();
            builder.Property(s => s.EndDateTime).IsRequired();
            builder.Property(s => s.CreatedAt).IsRequired();
            builder.Property(s => s.Capacity).IsRequired();

            builder.HasOne(s => s.Workspace)
                   .WithMany(w => w.Slots)
                   .HasForeignKey(s => s.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => new { s.WorkspaceId, s.StartDateTime });
        }
    }
}
