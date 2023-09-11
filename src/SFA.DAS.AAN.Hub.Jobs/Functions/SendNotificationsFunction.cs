using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Data;

namespace SFA.DAS.AAN.Hub.Jobs.Functions;

public class SendNotificationsFunction
{
    private readonly IAanDataContext _dataContext;

    public SendNotificationsFunction(IAanDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [FunctionName(nameof(SendNotificationsFunction))]
    public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timer,
        ILogger log)
    {
        log.LogInformation($"{nameof(SendNotificationsFunction)} has been triggered.");

        var nots = await _dataContext.Notifications.ToListAsync();

        log.LogInformation($"Processed {nots.Count} notifications.");
    }
}
