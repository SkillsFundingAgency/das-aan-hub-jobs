using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Interfaces
{
    public interface IApprenticeAccountsApi
    {
        Task PostValueAsync<T>(CancellationToken cancellationToken, string url, T data);

        HttpResponseMessage Response { get; }

        T GetDeserializedResponseObject<T>();

        string ResponseContent { get; }
    }
}
