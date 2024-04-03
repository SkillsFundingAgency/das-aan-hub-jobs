using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Api.Interfaces;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.HttpClientConfiguration
{
    [ExcludeFromCodeCoverage]
    public static class HttpClientConfigurationExtension
    {
        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IApprenticeAccountsApi, ApprenticeAccountsApi>(nameof(ApprenticeAccountsApi), client =>
            {
                string baseUrl = configuration["ApplicationConfiguration:ApprenticeAccountsApiBaseUrl"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new InvalidOperationException("Base URL for ApprenticeAccountsApi is missing in configuration.");
                }

                client.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }
}
