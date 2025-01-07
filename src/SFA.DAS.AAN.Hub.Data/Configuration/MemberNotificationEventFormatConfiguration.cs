using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Data.Configuration;

[ExcludeFromCodeCoverage]
public class MemberNotificationEventFormatConfiguration : IEntityTypeConfiguration<MemberNotificationEventFormat>
{
    public void Configure(EntityTypeBuilder<MemberNotificationEventFormat> builder)
    {
        builder.ToTable(nameof(MemberNotificationEventFormat));
        builder.HasKey(x => x.Id);
    }
}
