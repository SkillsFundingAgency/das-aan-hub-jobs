using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddApplicationRegistrations(this IServiceCollection services)
    {
        services.AddTransient<IEventSignUpNotificationService, EventSignUpNotificationService>();
        services.AddTransient<IEventNotificationService, EventNotificationService>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IEventQueryService, EventQueryService>();
        services.AddTransient<IMemberDataCleanupService, MemberDataCleanupService>();
        services.AddTransient<ISynchroniseApprenticeDetailsService, SynchroniseApprenticeDetailsService>();
        return services;
    }
}
