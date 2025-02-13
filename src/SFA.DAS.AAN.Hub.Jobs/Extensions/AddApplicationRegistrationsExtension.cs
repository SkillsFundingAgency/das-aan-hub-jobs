using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.Encoding;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.AAN.Hub.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class AddApplicationRegistrationsExtension
{
    public static IServiceCollection AddApplicationRegistrations(this IServiceCollection services)
    {
        services.AddTransient<IEventSignUpNotificationService, EventSignUpNotificationService>();
        services.AddTransient<IEventNotificationService, EventNotificationService>();
        services.AddTransient<IApprenticeEventNotificationService, ApprenticeEventNotificationService>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IEventQueryService, EventQueryService>();
        services.AddTransient<IApprenticeEventQueryService, ApprenticeEventQueryService>();
        services.AddTransient<IEmployerAccountsService, EmployerAccountsService>();
        services.AddTransient<IMemberDataCleanupService, MemberDataCleanupService>();
        services.AddTransient<ISynchroniseApprenticeDetailsService, SynchroniseApprenticeDetailsService>();

        return services;
    }

    public static IServiceCollection AddDasEncoding(this IServiceCollection services, IConfiguration configuration)
    {
        var dasEncodingConfig = new EncodingConfig { Encodings = [] };
        configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
        services.AddSingleton(dasEncodingConfig);
        services.AddSingleton<IEncodingService, EncodingService>();

        return services;
    }
}
