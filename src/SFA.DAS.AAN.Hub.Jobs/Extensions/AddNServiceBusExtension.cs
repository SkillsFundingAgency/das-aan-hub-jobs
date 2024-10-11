using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Extensions;

[ExcludeFromCodeCoverage]
internal static class AddNServiceBusExtension
{
    public const string NotificationsQueue = "SFA.DAS.Notifications.MessageHandlers";
    public const string EndpointName = "SFA.DAS.AAN.Hub.Jobs";
    private const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";
    public static void AddNServiceBus(this IServiceCollection services, IConfiguration configuration)
    {

        NServiceBusConfiguration nServiceBusConfiguration = new();
        configuration.GetSection(nameof(NServiceBusConfiguration)).Bind(nServiceBusConfiguration);
        
        var endpointConfiguration = new EndpointConfiguration(EndpointName);
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo($"{EndpointName}-errors");
        endpointConfiguration.Conventions();
        endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

        if (!string.IsNullOrEmpty(nServiceBusConfiguration.NServiceBusLicense))
        {
            endpointConfiguration.License(nServiceBusConfiguration.NServiceBusLicense);
        }

        endpointConfiguration.SendOnly();

        var startServiceBusEndpoint = false;

        if (configuration["EnvironmentName"] == "LOCAL")
        {
            var notificationJob = configuration["AzureWebJobs.SendNotificationsFunction.Disabled"];
            if (string.IsNullOrWhiteSpace(notificationJob) || notificationJob.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
                transport.Routing().RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
                var connectionString = nServiceBusConfiguration.NServiceBusConnectionString;
                transport.ConnectionString(connectionString);
                startServiceBusEndpoint = true;
            }
        }
        else
        {
            var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
            transport.ConnectionString(nServiceBusConfiguration.NServiceBusConnectionString);
            transport.Routing().RouteToEndpoint(typeof(SendEmailCommand), NotificationsMessageHandler);
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