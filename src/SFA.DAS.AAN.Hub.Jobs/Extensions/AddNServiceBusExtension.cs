using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Extensions;

[ExcludeFromCodeCoverage]
internal static class AddNServiceBusExtension
{
    public const string NotificationsQueue = "SFA.DAS.Notifications.MessageHandlers";
    public const string EndpointName = "SFA.DAS.AAN.Hub.Jobs";
    public static void AddNServiceBus(this IServiceCollection services, IConfiguration configuration)
    {

        NServiceBusConfiguration nServiceBusConfiguration = new();
        configuration.GetSection(nameof(NServiceBusConfiguration)).Bind(nServiceBusConfiguration);

        var endpointConfiguration = new EndpointConfiguration(EndpointName)
                .UseErrorQueue($"{EndpointName}-errors")
                //.UseInstallers()
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer();

        //var endpointConfiguration = new EndpointConfiguration(EndpointName);
        endpointConfiguration.EnableInstallers();
        //endpointConfiguration.SendFailedMessagesTo($"{EndpointName}-errors");
        //endpointConfiguration.Conventions();
        //endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

        if (!string.IsNullOrEmpty(nServiceBusConfiguration.NServiceBusLicense))
        {
            endpointConfiguration.UseLicense(nServiceBusConfiguration.NServiceBusLicense);
        }

        endpointConfiguration.SendOnly();

        var startServiceBusEndpoint = false;

        if (configuration["EnvironmentName"] == "LOCAL")
        {
            var notificationJob = configuration["AzureWebJobs.SendNotificationsFunction.Disabled"];
            if (string.IsNullOrWhiteSpace(notificationJob) || notificationJob.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                transport.Routing().RouteToEndpoint(typeof(SendEmailCommand), RoutingSettingsExtensions.NotificationsMessageHandler);
                var connectionString = nServiceBusConfiguration.NServiceBusConnectionString;
                transport.ConnectionString(connectionString);
                startServiceBusEndpoint = true;
            }
        }
        else
        {
            endpointConfiguration.UseAzureServiceBusTransport(nServiceBusConfiguration.NServiceBusConnectionString, s => s.AddRouting());
            startServiceBusEndpoint = true;
        }

        if (startServiceBusEndpoint)
        {
            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            services
                .AddSingleton(p => endpointInstance)
                .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>());
        }

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
