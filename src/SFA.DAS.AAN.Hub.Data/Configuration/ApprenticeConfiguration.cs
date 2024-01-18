using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Configuration;

[ExcludeFromCodeCoverage]
public class ApprenticeConfiguration : IEntityTypeConfiguration<Apprentice>
{
    public void Configure(EntityTypeBuilder<Apprentice> builder)
    {
        builder.ToTable(nameof(Apprentice));
        builder.HasKey(x => x.MemberId);
    }
}
