using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Data.Entities;
[ExcludeFromCodeCoverage]
public class Apprentice
{
    public Guid MemberId { get; set; }
    public Guid ApprenticeId { get; set; }
    public virtual Member Member { get; set; } = null!;
}
