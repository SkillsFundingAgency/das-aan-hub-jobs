using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AAN.Hub.Jobs.Functions;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Functions;

public class SendNotificationsFunctionTests
{
    [Test, MoqAutoData]
    public async Task Run_InvokesService(TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        Mock<INotificationService> serviceMock = new();
        Mock<ILogger<SendNotificationsFunction>> loggerMock = new();
        SendNotificationsFunction sut = new(serviceMock.Object, loggerMock.Object);

        await sut.Run(timerInfo, cancellationToken);

        serviceMock.Verify(s => s.ProcessNotificationBatch(cancellationToken));
    }
}
