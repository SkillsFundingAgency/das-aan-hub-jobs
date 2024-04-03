using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.AAN.Hub.Jobs.Api.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Api.Clients;

public class ApprenticeAccountsApi : IApprenticeAccountsApi, IDisposable
{
    private readonly ILogger<ApprenticeAccountsApi> _logger;

    private readonly IHttpClientFactory _httpClientFactory;
    public HttpResponseMessage Response { get; set; }
    public string ResponseContent => Response?.Content?.ReadAsStringAsync().Result;

    private bool isDisposed;

    private const string ApplicationJsonResponseType = "application/json";

    public ApprenticeAccountsApi(ILogger<ApprenticeAccountsApi> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task PostValueAsync<T>(CancellationToken cancellationToken, string url, T data)
    {
        var client = _httpClientFactory.CreateClient(nameof(ApprenticeAccountsApi));
        Response = await client.PostAsync(url, GetStringContent(data), cancellationToken);
    }

    public T GetDeserializedResponseObject<T>()
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(ResponseContent);
        }
        catch (JsonSerializationException _Exception)
        {
            _logger.LogError(_Exception, "Unable to deserialize response content for type {ContentType}", typeof(T).Name);
            throw;
        }
    }

    private StringContent GetStringContent(object obj) => new StringContent(JsonConvert.SerializeObject(obj), Encoding.Default, ApplicationJsonResponseType);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            Response?.Dispose();
        }

        isDisposed = true;
    }
}
