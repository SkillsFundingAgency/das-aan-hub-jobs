using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.NServiceBus.Extensions;

namespace SFA.DAS.AAN.Hub.Jobs.Extensions;

[ExcludeFromCodeCoverage]
internal static class AddNServiceBusExtension
{
    public const string NotificationsQueue = "SFA.DAS.Notifications.MessageHandlers";
    public const string EndpointName = "SFA.DAS.AAN.Hub.Jobs";
    public static IFunctionsHostBuilder AddNServiceBus(this IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;

        NServiceBusConfiguration nServiceBusConfiguration = new();
        configuration.GetSection(nameof(NServiceBusConfiguration)).Bind(nServiceBusConfiguration);

        var endpointConfiguration = new EndpointConfiguration(EndpointName);
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UseMessageConventions();
        endpointConfiguration.UseNewtonsoftJsonSerializer();
        endpointConfiguration.License(nServiceBusConfiguration.NServiceBusLicense);
        endpointConfiguration.SendOnly();

        var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
        transport.AddRouting(routeSettings =>
        {
            routeSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsQueue);
        });
        var connectionString = nServiceBusConfiguration.NServiceBusConnectionString;
        transport.ConnectionString(connectionString);

        var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

        builder.Services
            .AddSingleton(p => endpointInstance)
            .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>());

        return builder;
    }
}
