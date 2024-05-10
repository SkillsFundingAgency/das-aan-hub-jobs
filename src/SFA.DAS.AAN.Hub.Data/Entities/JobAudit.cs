using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Data.Entities
{
    [ExcludeFromCodeCoverage]
    public class JobAudit
    {
        public int Id { get; set; }
        public required string JobName { get; set; } = null!;
        public required DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Notes { get; set; }
    }
}
