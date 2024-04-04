using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Response
{
    [ExcludeFromCodeCoverage]
    public class ApprenticeSyncDto
    {
        public Guid ApprenticeID { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; } = null!;

        public DateTime LastUpdatedDate { get; set; }
    }
}
