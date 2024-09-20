using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Configuration;

[ExcludeFromCodeCoverage]
public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable(nameof(Attendance));
        builder.HasKey(x => x.Id);

        builder.HasOne(a => a.CalendarEvent)
            .WithMany()
            .HasForeignKey(a => a.CalendarEventId);
    }
}
