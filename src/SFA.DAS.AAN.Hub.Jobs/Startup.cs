using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SFA.DAS.AAN.Hub.Jobs;
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
        var sqlConnectionString = builder.GetContext().Configuration["SqlConnectionString"];
        var environmentName = builder.GetContext().Configuration["EnvironmentName"];
        builder.Services.AddAanDataContext(sqlConnectionString, environmentName);
    }
}
