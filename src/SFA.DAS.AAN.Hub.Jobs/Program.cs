using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.AAN.Hub.Data.Extensions;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.AAN.Hub.Jobs.Extensions;
using SFA.DAS.AAN.Hub.Jobs.HttpClientConfiguration;
using SFA.DAS.Encoding;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(
        builder =>
        {
            builder.AddConfiguration();
        })
    .ConfigureServices((context, s) =>
    {
        s
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddOptions()
            .Configure<ApplicationConfiguration>(context.Configuration.GetSection(nameof(ApplicationConfiguration)))
            .AddAanDataContext(context.Configuration)
            .ConfigureHttpClients(context.Configuration)
            .AddApplicationRegistrations()
            .AddNServiceBus(context.Configuration);
        s.AddSingleton<IEncodingService, EncodingService>();
    })
    .Build();

await host.RunAsync();
