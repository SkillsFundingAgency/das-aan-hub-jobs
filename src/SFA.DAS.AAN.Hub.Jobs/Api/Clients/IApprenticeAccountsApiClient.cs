using RestEase;
using SFA.DAS.AAN.Hub.Jobs.Api.Response;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Clients
{
    public interface IApprenticeAccountsApiClient
    {
        [Post("apprentices/sync")]
        [AllowAnyStatusCode]
        Task<Response<ApprenticeSyncResponseDto>> SynchroniseApprentices([Body] Guid[] apprenticeIds, [Query] DateTime? updatedSinceDate, CancellationToken cancellationToken);
    }
}
