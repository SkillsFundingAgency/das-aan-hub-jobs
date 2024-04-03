using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Services;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services.Apprentices
{
    public class WhenSynchronisingApprenticeDetails
    {
        private Mock<IApprenticeRepository> _apprenticeRepositoryMock = null!;
        private Mock<IJobAuditRepository> _jobAuditRepositoryMock = null!;
        private Mock<IMemberRepository> _memberRepositoryMock = null!;
        private Mock<ILogger<SynchroniseApprenticeDetailsService>> _loggerMock = new();
        private CancellationToken _cancellationToken;

        [SetUp]
        public void Init()
        {
            Fixture fixture = new();
            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _cancellationToken = fixture.Create<CancellationToken>();
            _loggerMock = new Mock<ILogger<SynchroniseApprenticeDetailsService>>();
        }

        //public SynchroniseApprenticeDetailsService CreateService(
        //    ILogger<SynchroniseApprenticeDetailsService> logger,
        //    IApprenticeAccountsApi apprenticeAccountsApi,
        //    IApprenticeRepository apprenticeshipRespository,
        //    IJobAuditRepository jobAuditRepository,
        //    IMemberRepository memberRepository
        //)
        //{
        //    return new SynchroniseApprenticeDetailsService(logger, apprenticeAccountsApi, apprenticeshipRespository, jobAuditRepository, memberRepository);
        //}

        //[Test]
        //public async Task AndApprenticesNull_ThenLogsAuditAndReturnsDefault()
        //{
        //    SetupEmptyMocks();

        //    var sut = CreateService(
        //        _loggerMock.Object,
        //        _apprenticeAccountsApiMock.Object,
        //        _apprenticeRepositoryMock.Object,
        //        _jobAuditRepositoryMock.Object,
        //        _memberRepositoryMock.Object
        //    );

        //    var result = await sut.SynchroniseApprentices(_cancellationToken);

        //    _jobAuditRepositoryMock.Verify(x => x.RecordAudit(_cancellationToken, It.IsAny<JobAudit>()), Times.Exactly(1));

        //    Assert.That(result, Is.EqualTo(0));
        //}

        //[Test]
        //public async Task AndApprenticesEmpty_ThenLogsAuditAndReturnsDefault()
        //{
        //    SetupEmptyMocks();

        //    _apprenticeRepositoryMock.Setup(a => a.GetApprentices(_cancellationToken)).ReturnsAsync(new List<Apprentice>());

        //    var sut = CreateService(
        //        _loggerMock.Object,
        //        _apprenticeAccountsApiMock.Object,
        //        _apprenticeRepositoryMock.Object,
        //        _jobAuditRepositoryMock.Object,
        //        _memberRepositoryMock.Object
        //    );

        //    var result = await sut.SynchroniseApprentices(_cancellationToken);

        //    _jobAuditRepositoryMock.Verify(x => x.RecordAudit(_cancellationToken, It.IsAny<JobAudit>()), Times.Exactly(1));

        //    Assert.That(result, Is.EqualTo(0));
        //}

        //[Test]
        //public async Task AndLastJobAuditNull_ThenTheRequestForApprenticesContainsNoDate()
        //{
        //    SetupEmptyMocks();

        //    var apprentices = new List<Apprentice>
        //    {
        //        new Apprentice { ApprenticeId = new Guid() },
        //        new Apprentice { ApprenticeId = new Guid() }
        //    };

        //    JobAudit? lastJobAudit = null;

        //    _jobAuditRepositoryMock.Setup(x => x.GetMostRecentJobAudit(_cancellationToken))
        //        .ReturnsAsync(lastJobAudit);

        //    _apprenticeRepositoryMock.Setup(x => x.GetApprentices(_cancellationToken))
        //        .ReturnsAsync(apprentices);

        //    _apprenticeAccountsApiMock.Setup(x => x.PostValueAsync(
        //        It.IsAny<CancellationToken>(),
        //        "apprentices/sync",
        //        It.IsAny<List<Guid>>()
        //    )).Verifiable();

        //    var sut = CreateService(
        //        _loggerMock.Object,
        //        _apprenticeAccountsApiMock.Object,
        //        _apprenticeRepositoryMock.Object,
        //        _jobAuditRepositoryMock.Object,
        //        _memberRepositoryMock.Object
        //    );

        //    await sut.SynchroniseApprentices(_cancellationToken);

        //    _apprenticeAccountsApiMock.VerifyAll();
        //}

        //[Test]
        //public async Task AndLastJobAuditNotNull_ThenTheRequestForApprenticesContainsTheDate()
        //{
        //    SetupEmptyMocks();

        //    var apprentices = new List<Apprentice>
        //    {
        //        new Apprentice { ApprenticeId = new Guid() },
        //        new Apprentice { ApprenticeId = new Guid() }
        //    };

        //    var lastJobAudit = new JobAudit()
        //    {
        //        JobName = nameof(SynchroniseApprenticeDetailsFunction),
        //        StartTime = DateTime.UtcNow.AddDays(-1)
        //    };

        //    _jobAuditRepositoryMock.Setup(x => x.GetMostRecentJobAudit(_cancellationToken)).ReturnsAsync(lastJobAudit);
        //    _apprenticeRepositoryMock.Setup(x => x.GetApprentices(_cancellationToken)).ReturnsAsync(apprentices);

        //    _apprenticeAccountsApiMock.Setup(x => x.PostValueAsync(
        //        It.IsAny<CancellationToken>(),
        //        "apprentices/sync?updatedSinceDate=" + lastJobAudit.StartTime.ToString("yyyy-MM-dd"),
        //        It.IsAny<List<Guid>>()
        //    )).Verifiable();

        //    var sut = CreateService(
        //        _loggerMock.Object,
        //        _apprenticeAccountsApiMock.Object,
        //        _apprenticeRepositoryMock.Object,
        //        _jobAuditRepositoryMock.Object,
        //        _memberRepositoryMock.Object
        //    );

        //    await sut.SynchroniseApprentices(_cancellationToken);

        //    _apprenticeAccountsApiMock.VerifyAll();
        //}

        //[Test]
        //public async Task AndAuditQueryFails_ThenExceptionIsThrown()
        //{
        //    var apprentices = new List<Apprentice>
        //    {
        //        new Apprentice { ApprenticeId = new Guid() },
        //        new Apprentice { ApprenticeId = new Guid() }
        //    };

        //    SetupEmptyMocks();

        //    _apprenticeRepositoryMock.Setup(x => x.GetApprentices(_cancellationToken)).ReturnsAsync(apprentices);

        //    var sut = CreateService(
        //        _loggerMock.Object,
        //        _apprenticeAccountsApiMock.Object,
        //        _apprenticeRepositoryMock.Object,
        //        _jobAuditRepositoryMock.Object,
        //        _memberRepositoryMock.Object
        //    );

        //    _jobAuditRepositoryMock
        //        .Setup(x => x.GetMostRecentJobAudit(_cancellationToken))
        //        .ThrowsAsync(new Exception(nameof(Exception)));

        //    var result = await sut.SynchroniseApprentices(_cancellationToken);

        //    _jobAuditRepositoryMock.Verify(x => x.RecordAudit(_cancellationToken, It.IsAny<JobAudit>()), Times.Exactly(1));

        //    Assert.That(result, Is.EqualTo(0));
        //}

        private void SetupEmptyMocks()
        {
            _apprenticeRepositoryMock = new Mock<IApprenticeRepository>();
            _jobAuditRepositoryMock = new Mock<IJobAuditRepository>();
            _memberRepositoryMock = new Mock<IMemberRepository>();
        }
    }
}
