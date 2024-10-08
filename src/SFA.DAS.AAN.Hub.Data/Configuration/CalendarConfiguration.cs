﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Configuration;
[ExcludeFromCodeCoverage]
public class CalendarConfiguration : IEntityTypeConfiguration<Calendar>
{
    public void Configure(EntityTypeBuilder<Calendar> builder)
    {
        builder.ToTable(nameof(Calendar));
        builder.HasKey(x => x.Id);
    }
}