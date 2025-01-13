using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Data.Configuration;

[ExcludeFromCodeCoverage]
public class MemberNotificationLocationConfiguration : IEntityTypeConfiguration<MemberNotificationLocation>
{
    public void Configure(EntityTypeBuilder<MemberNotificationLocation> builder)
    {
        builder.ToTable(nameof(MemberNotificationLocation));
        builder.HasKey(x => x.Id);
    }
}
