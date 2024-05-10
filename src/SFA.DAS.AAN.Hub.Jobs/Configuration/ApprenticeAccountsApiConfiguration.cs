using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ApprenticeAccountsApiConfiguration
    {
        public string Url { get; set; } = null!;
        public string Identifier { get; set; } = null!;
    }
}
