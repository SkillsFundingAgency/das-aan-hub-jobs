using System.Threading.Tasks;
using SFA.DAS.AAN.Hub.Jobs.Models;

namespace SFA.DAS.AAN.Hub.Jobs.Services;

public interface IMessagingService
{
    Task SendMessage(SendEmailCommand command);
}

public class MessagingService : IMessagingService
{
    public async Task SendMessage(SendEmailCommand command) => await Task.CompletedTask;
}
