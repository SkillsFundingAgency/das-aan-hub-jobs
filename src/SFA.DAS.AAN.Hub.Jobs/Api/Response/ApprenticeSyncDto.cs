using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Response
{
    [ExcludeFromCodeCoverage]
    public class ApprenticeSyncDto
    {
        [JsonProperty("apprenticeId")]
        public Guid ApprenticeID { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; } = null!;

        [JsonProperty("lastName")]
        public string LastName { get; set; } = null!;

        [JsonProperty("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; } = null!;

        [JsonProperty("lastUpdatedDate")]
        public DateTime LastUpdatedDate { get; set; }
    }
}
