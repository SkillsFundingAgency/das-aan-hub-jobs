using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Configuration
{
    [ExcludeFromCodeCoverage]
    public class AanOuterApiConfiguration
    {
        public string ApiBaseUrl { get; set; } = null!;
        public string SubscriptionKey { get; set; } = null!;
        public string ApiVersion { get; set; } = null!;
    }
}
