using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Authentication;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Http.Configuration;
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
            AddApprenticeOuterApiClient(services, configuration);

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

            services.AddTransient<IApimClientConfiguration>((_) => outerApiConfig);

            services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
            services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
            services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

            services.AddRestEaseClient<IOuterApiClient>(outerApiConfig.ApiBaseUrl)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();
        }

        private static void AddApprenticeOuterApiClient(IServiceCollection services, IConfiguration configuration)
        {
            var outerApiConfig = configuration.GetSection(nameof(ApplicationConfiguration)).Get<ApplicationConfiguration>().ApprenticeAanOuterApiConfiguration;

            services.AddTransient<IApimClientConfiguration>((_) => outerApiConfig);

            services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
            services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
            services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

            services.AddRestEaseClient<IApprenticeOuterApiClient>(outerApiConfig.ApiBaseUrl)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();
        }
    }
}
