using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Response
{
    [ExcludeFromCodeCoverage]
    public class ApprenticeSyncResponseDto
    {
        public ApprenticeSyncResponseDto() => Apprentices = Array.Empty<ApprenticeSyncDto>();
        public ApprenticeSyncResponseDto(ApprenticeSyncDto[] apprentices) => Apprentices = apprentices;

        public ApprenticeSyncDto[] Apprentices { get; set; }
    }
}
