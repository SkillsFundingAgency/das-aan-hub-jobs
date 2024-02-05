using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AAN.Hub.Data.Repositories;

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

    public static IServiceCollection AddAanDataContext(this IServiceCollection services, string connectionString, string environmentName)
    {
        services.AddDbContext<AanDataContext>((serviceProvider, options) =>
        {
            SqlConnection connection = null!;

            if (!environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                connection = new SqlConnection
                {
                    ConnectionString = connectionString,
                    AccessToken = AzureTokenProvider.GetToken(new TokenRequestContext(scopes: new string[] { AzureResource })).Token
                };
            }
            else
            {
                connection = new SqlConnection(connectionString);
            }

            options.UseSqlServer(
                connection,
                o => o.CommandTimeout((int)TimeSpan.FromMinutes(5).TotalSeconds));
        });

        services.AddScoped<IAanDataContext, AanDataContext>(provider => provider.GetService<AanDataContext>()!);
        services.AddScoped<INotificationsRepository, NotificationsRepository>();
        services.AddScoped<IMemberDataCleanupRepository, MemberDataCleanupRepository>();
        return services;
    }
}
