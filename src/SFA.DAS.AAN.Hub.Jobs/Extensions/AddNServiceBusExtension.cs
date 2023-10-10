using Microsoft.Azure.Functions.Extensions.DependencyInjection;

namespace SFA.DAS.AAN.Hub.Jobs.Extensions;
internal class AddNServiceBusExtension
{
    private void ConfigureNServiceBus(IFunctionsHostBuilder builder)
    {
        //var logger = LoggerFactory.Create(b => b.ConfigureLogging()).CreateLogger<Startup>();

        //builder.UseNServiceBus((IConfiguration appConfiguration) =>
        //{
        //    var configuration = new ServiceBusTriggeredEndpointConfiguration(EndpointName);
        //    var connectionStringConfiguration = ServiceBusConnectionConfiguration.GetServiceBusConnectionString(appConfiguration);

        //    if (connectionStringConfiguration.ConnectionType == ServiceBusConnectionConfiguration.ConnectionAuthenticationType.ManagedIdentity)
        //    {
        //        configuration.Transport.ConnectionString(connectionStringConfiguration.ConnectionString);
        //        configuration.Transport.CustomTokenCredential(new DefaultAzureCredential());
        //    }
        //    else
        //    {
        //        //Shared Access Key, Will pick up the AzureServiceJobsServiceBus Setting by Default.
        //    }

        //    var nServiceBusConfig = appConfiguration.GetSection("NServiceBusConfiguration").Get<NServiceBusConfiguration>();
        //    if (!string.IsNullOrWhiteSpace(nServiceBusConfig.License))
        //    {
        //        configuration.AdvancedConfiguration.License(nServiceBusConfig.License);
        //    }

        //    configuration.AdvancedConfiguration.SendFailedMessagesTo($"{EndpointName}-error");
        //    configuration.LogDiagnostics();

        //    configuration.AdvancedConfiguration.Conventions()
        //        .DefiningMessagesAs(IsMessage)
        //        .DefiningEventsAs(IsEvent)
        //        .DefiningCommandsAs(IsCommand);

        //    configuration.Transport.SubscriptionRuleNamingConvention(AzureQueueNameShortener.Shorten);

        //    configuration.AdvancedConfiguration.Pipeline.Register(new LogIncomingBehaviour(), nameof(LogIncomingBehaviour));
        //    configuration.AdvancedConfiguration.Pipeline.Register(new LogOutgoingBehaviour(), nameof(LogOutgoingBehaviour));

        //    var persistence = configuration.AdvancedConfiguration.UsePersistence<AzureTablePersistence>();
        //    persistence.ConnectionString(appConfiguration.GetConnectionStringOrSetting("AzureWebJobsStorage"));
        //    configuration.AdvancedConfiguration.EnableInstallers();

        //    return configuration;
        //});
    }
}
