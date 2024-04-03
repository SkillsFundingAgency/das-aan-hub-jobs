using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Data.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Data.Extensions;

[ExcludeFromCodeCoverage]
public static class AddAanDataContextExtension
{
    private static readonly string AzureResource = "https://database.windows.net/";

    private static readonly ChainedTokenCredential AzureTokenProvider = new ChainedTokenCredential(
        new ManagedIdentityCredential(),
        new AzureCliCredential(),
        new VisualStudioCodeCredential(),
        new VisualStudioCredential()
    );

    public static IServiceCollection AddAanDataContext(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlConnectionString = configuration["SqlConnectionString"]!;
        var environmentName = configuration["EnvironmentName"]!;

        services.AddDbContext<AanDataContext>((serviceProvider, options) =>
        {
            SqlConnection connection = null!;

            if (!environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                connection = new SqlConnection
                {
                    ConnectionString = sqlConnectionString,
                    AccessToken = AzureTokenProvider.GetToken(new TokenRequestContext(scopes: new string[] { AzureResource })).Token
                };
            }
            else
            {
                connection = new SqlConnection(sqlConnectionString);
            }

            options.UseSqlServer(
                connection,
                o => o.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds));
        });

        services.AddScoped<IAanDataContext, AanDataContext>(provider => provider.GetService<AanDataContext>()!);
        services.AddScoped<INotificationsRepository, NotificationsRepository>();
        services.AddScoped<IMemberDataCleanupRepository, MemberDataCleanupRepository>();
        services.AddScoped<IApprenticeRepository, ApprenticeRepository>();
        services.AddScoped<IJobAuditRepository, JobAuditRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        return services;
    }
}
