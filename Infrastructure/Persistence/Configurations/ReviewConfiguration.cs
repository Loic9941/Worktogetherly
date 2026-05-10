using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating).IsRequired();
            builder.Property(r => r.Comment).HasMaxLength(2000);
            builder.Property(r => r.CreatedAt).IsRequired();

            builder.HasIndex(r => r.BookingId).IsUnique();
            builder.HasIndex(r => new { r.ReviewerId, r.WorkspaceId }).IsUnique();

            builder.HasOne(r => r.Booking)
                   .WithOne(b => b.Review)
                   .HasForeignKey<Review>(r => r.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Reviewer)
                   .WithMany(u => u.Reviews)
                   .HasForeignKey(r => r.ReviewerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Workspace)
                   .WithMany(w => w.Reviews)
                   .HasForeignKey(r => r.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
