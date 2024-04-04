using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.AAN.Hub.Jobs.Functions
{
    public class SynchroniseApprenticeDetailsFunction
    {
        private readonly ILogger<SynchroniseApprenticeDetailsFunction> _logger;

        private readonly ISynchroniseApprenticeDetailsService _synchroniseApprenticeDetailsService;

        public SynchroniseApprenticeDetailsFunction(ILogger<SynchroniseApprenticeDetailsFunction> logger, ISynchroniseApprenticeDetailsService synchroniseApprenticeDetailsService)
        {
            _synchroniseApprenticeDetailsService = synchroniseApprenticeDetailsService;
            _logger = logger;
        }

        [Function(nameof(SynchroniseApprenticeDetailsFunction))]
        public async Task Run([TimerTrigger("%SynchroniseApprenticeDetailsFunctionSchedule%", RunOnStartup = true)] TimerInfo timer, CancellationToken cancellationToken)
        {
            _logger.LogInformation("{MethodName} has been triggered.", nameof(SynchroniseApprenticeDetailsFunction));

            var count = await _synchroniseApprenticeDetailsService.SynchroniseApprentices(cancellationToken);

            _logger.LogInformation("Processed {ApprenticeCount} apprentices.", count);
        }
    }
}
