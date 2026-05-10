using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations
{
    public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.ToTable("Workspaces");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
            builder.Property(w => w.Description).IsRequired().HasMaxLength(2000);
            builder.Property(w => w.Address).IsRequired().HasMaxLength(500);
            builder.Property(w => w.Capacity).IsRequired();
            builder.Property(w => w.IsActive).IsRequired();
            builder.Property(w => w.CreatedAt).IsRequired();
            builder.Property(w => w.Latitude).IsRequired();
            builder.Property(w => w.Longitude).IsRequired();
            builder.Property(w => w.PhotoPath).HasMaxLength(500).IsRequired(false);

            builder.HasOne(w => w.Owner)
                   .WithMany(u => u.Workspaces)
                   .HasForeignKey(w => w.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
