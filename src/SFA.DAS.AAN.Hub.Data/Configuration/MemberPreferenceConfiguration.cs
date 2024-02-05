using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Configuration;

[ExcludeFromCodeCoverage]
public class MemberPreferenceConfiguration : IEntityTypeConfiguration<MemberPreference>
{
    public void Configure(EntityTypeBuilder<MemberPreference> builder)
    {
        builder.ToTable(nameof(MemberPreference));
        builder.HasKey(x => x.Id);
    }
}
