using SFA.DAS.Http.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Configuration
{
    [ExcludeFromCodeCoverage]
    public class AanOuterApiConfiguration : IApimClientConfiguration
    {
        public string ApiBaseUrl { get; set; } = null!;
        public string SubscriptionKey { get; set; } = null!;
        public string ApiVersion { get; set; } = null!;
    }
}
