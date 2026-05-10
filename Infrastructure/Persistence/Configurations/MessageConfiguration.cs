using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("Messages");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Content).IsRequired().HasMaxLength(2000);
            builder.Property(m => m.IsRead).IsRequired();
            builder.Property(m => m.CreatedAt).IsRequired();

            builder.HasIndex(m => m.RecipientId);
            builder.HasIndex(m => new { m.RecipientId, m.IsRead });

            builder.HasOne(m => m.Sender)
                   .WithMany(u => u.SentMessages)
                   .HasForeignKey(m => m.SenderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Recipient)
                   .WithMany(u => u.ReceivedMessages)
                   .HasForeignKey(m => m.RecipientId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
