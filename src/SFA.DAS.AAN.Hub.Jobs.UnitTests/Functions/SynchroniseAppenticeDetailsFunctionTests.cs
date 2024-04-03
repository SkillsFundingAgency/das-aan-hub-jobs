using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AAN.Hub.Jobs.Functions;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Functions
{
    public class SynchroniseAppenticeDetailsFunctionTests
    {
        [Test]
        [MoqAutoData]
        public async Task Run_InvokesService(TimerInfo timerInfo, CancellationToken cancellationToken)
        {
            Mock<ISynchroniseApprenticeDetailsService> serviceMock = new();
            Mock<ILogger<SynchroniseApprenticeDetailsFunction>> loggerMock = new();
            SynchroniseApprenticeDetailsFunction sut = new(loggerMock.Object, serviceMock.Object);

            await sut.Run(timerInfo, cancellationToken);

            serviceMock.Verify(s => s.SynchroniseApprentices(cancellationToken));
        }
    }
}
