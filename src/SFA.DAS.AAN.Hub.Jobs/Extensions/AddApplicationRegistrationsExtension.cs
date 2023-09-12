using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.AAN.Hub.Data;

namespace SFA.DAS.AAN.Hub.Jobs.Extensions;

public static class AddApplicationRegistrationsExtension
{
    public static void AddApplicationRegistrations(IServiceCollection services)
    {
        services.AddTransient<IAanDataContext, AanDataContext>();
    }
}
