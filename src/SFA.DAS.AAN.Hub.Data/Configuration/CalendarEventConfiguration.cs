using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Configuration;
[ExcludeFromCodeCoverage]
public class CalendarEventConfiguration : IEntityTypeConfiguration<CalendarEvent>
{
    public void Configure(EntityTypeBuilder<CalendarEvent> builder)
    {
        builder.ToTable(nameof(CalendarEvent));
        builder.HasKey(x => x.Id);

        builder.HasOne(ce => ce.Calender)
            .WithOne()
            .HasForeignKey<CalendarEvent>(ce => ce.CalendarId);

        builder.HasOne(cm => cm.Member)
            .WithOne()
            .HasForeignKey<CalendarEvent>(cm => cm.CreatedByMemberId);
    }
}