using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AAN.Hub.Data.Extensions;
using SFA.DAS.AAN.Hub.Jobs;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.AAN.Hub.Jobs.Extensions;

[assembly: FunctionsStartup(typeof(Startup))]

namespace SFA.DAS.AAN.Hub.Jobs;

[ExcludeFromCodeCoverage]
public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.AddConfiguration();
    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = builder.GetContext().Configuration;
        var sqlConnectionString = configuration["SqlConnectionString"];
        var environmentName = configuration["EnvironmentName"];

        builder.Services.AddOptions();
        builder.Services.AddAanDataContext(sqlConnectionString, environmentName);
        builder.Services.AddApplicationRegistrations();

        builder.Services.Configure<ApplicationConfiguration>(configuration.GetSection(nameof(ApplicationConfiguration)));
    }
}
