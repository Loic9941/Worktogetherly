using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations;

public class BookingMaterialConfiguration : IEntityTypeConfiguration<BookingMaterial>
{
    public void Configure(EntityTypeBuilder<BookingMaterial> builder)
    {
        builder.ToTable("BookingMaterials");
        builder.HasKey(bm => new { bm.BookingId, bm.MaterialId });

        builder.HasOne(bm => bm.Booking)
               .WithMany(b => b.BookingMaterials)
               .HasForeignKey(bm => bm.BookingId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bm => bm.Material)
               .WithMany()
               .HasForeignKey(bm => bm.MaterialId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
