using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AAN.Hub.Jobs.Functions;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Functions;

public class ApprenticeEventNotificationFunctionTests
{
    [Test, MoqAutoData]
    public async Task Run_InvokesService(TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        Mock<IApprenticeEventNotificationService> serviceMock = new();
        Mock<ILogger<ApprenticeEventNotificationsFunction>> logger = new();
        ApprenticeEventNotificationsFunction sut = new ApprenticeEventNotificationsFunction(serviceMock.Object, logger.Object);

        await sut.Run(timerInfo, cancellationToken);

        serviceMock.Verify(x => x.ProcessEventNotifications(cancellationToken));
    }
}
