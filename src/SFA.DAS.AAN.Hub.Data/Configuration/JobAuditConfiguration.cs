using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Configuration
{
    public class JobAuditConfiguration : IEntityTypeConfiguration<JobAudit>
    {
        public void Configure(EntityTypeBuilder<JobAudit> builder)
        {
            builder.ToTable(nameof(JobAudit));
            builder.HasKey(x => x.Id);
        }
    }
}