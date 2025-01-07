using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Authentication;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.HttpClientConfiguration
{
    [ExcludeFromCodeCoverage]
    public static class HttpClientConfigurationExtension
    {
        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();

            AddApprenticeAccountsApiClient(services, configuration);
            AddOuterApiClient(services, configuration);

            return services;
        }

        private static void AddApprenticeAccountsApiClient(IServiceCollection services, IConfiguration configuration)
        {
            var apiConfig = configuration.GetSection(nameof(ApplicationConfiguration)).Get<ApplicationConfiguration>().ApprenticeAccountsApiConfiguration;

            services.AddRestEaseClient<IApprenticeAccountsApiClient>(apiConfig.Url)
                .AddHttpMessageHandler(() => new InnerApiAuthenticationHeaderHandler(new AzureClientCredentialHelper(), apiConfig.Identifier));
        }

        private static void AddOuterApiClient(IServiceCollection services, IConfiguration configuration)
        {
            var outerApiConfig = configuration.GetSection(nameof(ApplicationConfiguration)).Get<ApplicationConfiguration>().AanOuterApiConfiguration;

            services.AddRestEaseClient<IOuterApiClient>(outerApiConfig.ApiBaseUrl);
        }
    }
}
