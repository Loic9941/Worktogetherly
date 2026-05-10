using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Infrastructure.Persistence.Configurations;

public class WorkspaceRuleConfiguration : IEntityTypeConfiguration<WorkspaceRule>
{
    public void Configure(EntityTypeBuilder<WorkspaceRule> builder)
    {
        builder.ToTable("WorkspaceRules");
        builder.HasKey(wr => new { wr.WorkspaceId, wr.RuleId });

        builder.HasOne(wr => wr.Workspace)
               .WithMany(w => w.WorkspaceRules)
               .HasForeignKey(wr => wr.WorkspaceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wr => wr.Rule)
               .WithMany(r => r.WorkspaceRules)
               .HasForeignKey(wr => wr.RuleId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
