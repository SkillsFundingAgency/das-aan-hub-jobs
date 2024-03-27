using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Functions
{
    public class SynchroniseApprenticeDetailsFunction
    {
        private readonly ISynchroniseApprenticeDetailsService _synchroniseApprenticeDetailsService;

        public SynchroniseApprenticeDetailsFunction(ISynchroniseApprenticeDetailsService synchroniseApprenticeDetailsService)
        {
            _synchroniseApprenticeDetailsService = synchroniseApprenticeDetailsService;
        }

        [FunctionName(nameof(SynchroniseApprenticeDetailsFunction))]
        public async Task Run([TimerTrigger("?", RunOnStartup = true)] TimerInfo timer, ILogger log, CancellationToken cancellationToken)
        {
            log.LogInformation("{MethodName} has been triggered.", nameof(SynchroniseApprenticeDetailsFunction));

            var count = await _synchroniseApprenticeDetailsService.SynchroniseApprentices(log, cancellationToken);

            log.LogInformation("Processed {ApprenticeCount} apprentices.", count);
        }
    }
}
