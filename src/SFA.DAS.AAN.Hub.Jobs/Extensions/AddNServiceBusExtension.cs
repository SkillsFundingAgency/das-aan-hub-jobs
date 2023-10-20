using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;

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

        var endpointConfiguration = new EndpointConfiguration(EndpointName)
                .UseErrorQueue($"{EndpointName}-errors")
                .UseInstallers()
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer();

        if (!string.IsNullOrEmpty(nServiceBusConfiguration.NServiceBusLicense))
        {
            endpointConfiguration.UseLicense(nServiceBusConfiguration.NServiceBusLicense);
        }

        endpointConfiguration.SendOnly();

#if DEBUG
        var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
        transport.Routing().RouteToEndpoint(typeof(SendEmailCommand), RoutingSettingsExtensions.NotificationsMessageHandler);
        var connectionString = nServiceBusConfiguration.NServiceBusConnectionString;
        transport.ConnectionString(connectionString);
#else

        endpointConfiguration.UseAzureServiceBusTransport(nServiceBusConfiguration.NServiceBusConnectionString, s => s.AddRouting());
#endif
        var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

        builder.Services
            .AddSingleton(p => endpointInstance)
            .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>());
#if !DEBUG
        builder.Services.AddHostedService<NServiceBusHostedService>();
#endif

        return builder;
    }
}

public static class RoutingSettingsExtensions
{
    public const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";

    public static void AddRouting(this RoutingSettings routingSettings)
    {
        routingSettings.RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
    }
}
