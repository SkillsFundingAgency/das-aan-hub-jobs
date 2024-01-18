﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Configuration;

[ExcludeFromCodeCoverage]
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable(nameof(Member));
        builder.HasKey(x => x.Id);
        builder.Property(m => m.UserType).HasConversion(new EnumToStringConverter<UserType>());
        builder.HasMany(m => m.MemberProfiles).WithOne(mp => mp.Member);
        builder.HasMany(m => m.MemberPreferences).WithOne(mp => mp.Member);
        builder.HasMany(m => m.Notifications).WithOne(n => n.Member);
        builder.HasMany(m => m.Audits).WithOne(a => a.Member).HasForeignKey(a => a.ActionedBy);
    }
}
