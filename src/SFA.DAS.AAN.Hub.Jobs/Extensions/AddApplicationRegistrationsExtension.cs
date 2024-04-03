using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Api.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddApplicationRegistrations(this IServiceCollection services)
    {
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IMemberDataCleanupService, MemberDataCleanupService>();
        services.AddTransient<ISynchroniseApprenticeDetailsService, SynchroniseApprenticeDetailsService>();
        services.AddTransient<IApprenticeAccountsApi, ApprenticeAccountsApi>();
        return services;
    }
}
