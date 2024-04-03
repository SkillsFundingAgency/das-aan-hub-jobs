using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RestEase;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Api.Response;
using SFA.DAS.AAN.Hub.Jobs.Functions;
using SFA.DAS.AAN.Hub.Jobs.Services;
using System.Net;
using System.Text;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services.Apprentices
{
    public class WhenSynchronisingApprenticeDetails
    {
        private Mock<IApprenticeRepository> _apprenticeRepositoryMock = null!;
        private Mock<IJobAuditRepository> _jobAuditRepositoryMock = null!;
        private Mock<IMemberRepository> _memberRepositoryMock = null!;
        private Mock<IApprenticeAccountsApiClient> _apprenticeAccountsApiClientMock = null!;
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

        public SynchroniseApprenticeDetailsService CreateService(
            ILogger<SynchroniseApprenticeDetailsService> logger,
            IApprenticeAccountsApiClient apprenticeAccountsApiClientMock,
            IApprenticeRepository apprenticeshipRespository,
            IJobAuditRepository jobAuditRepository,
            IMemberRepository memberRepository
        )
        {
            return new SynchroniseApprenticeDetailsService(logger, apprenticeAccountsApiClientMock, apprenticeshipRespository, jobAuditRepository, memberRepository);
        }

        [Test]
        public async Task AndApprenticesNull_ThenLogsAuditAndReturnsDefault()
        {
            SetupEmptyMocks();

            var sut = CreateService(
                _loggerMock.Object,
                _apprenticeAccountsApiClientMock.Object,
                _apprenticeRepositoryMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object
            );

            var result = await sut.SynchroniseApprentices(_cancellationToken);

            _jobAuditRepositoryMock.Verify(x => x.RecordAudit(_cancellationToken, It.IsAny<JobAudit>()), Times.Exactly(1));

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task AndApprenticesEmpty_ThenLogsAuditAndReturnsDefault()
        {
            SetupEmptyMocks();

            _apprenticeRepositoryMock.Setup(a => a.GetApprentices(_cancellationToken)).ReturnsAsync(new List<Apprentice>());

            var sut = CreateService(
                _loggerMock.Object,
                _apprenticeAccountsApiClientMock.Object,
                _apprenticeRepositoryMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object
            );

            var result = await sut.SynchroniseApprentices(_cancellationToken);

            _jobAuditRepositoryMock.Verify(x => x.RecordAudit(_cancellationToken, It.IsAny<JobAudit>()), Times.Exactly(1));

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task AndLastJobAuditNull_ThenTheRequestForApprenticesContainsNoDate()
        {
            SetupEmptyMocks();

            var apprentices = new List<Apprentice>
            {
                new Apprentice { ApprenticeId = Guid.NewGuid() },
                new Apprentice { ApprenticeId = Guid.NewGuid() }
            };

            var apprenticeIds = apprentices.Select(a => a.ApprenticeId).ToArray();

            JobAudit? lastJobAudit = null;

            _jobAuditRepositoryMock.Setup(x => x.GetMostRecentJobAudit(_cancellationToken))
                .ReturnsAsync(lastJobAudit);

            _apprenticeRepositoryMock.Setup(x => x.GetApprentices(_cancellationToken))
                .ReturnsAsync(apprentices);

            _apprenticeAccountsApiClientMock.Setup(x => x.SynchroniseApprentices(
                apprenticeIds,
                null,
                _cancellationToken
            )).Verifiable();

            var sut = CreateService(
                _loggerMock.Object,
                _apprenticeAccountsApiClientMock.Object,
                _apprenticeRepositoryMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object
            );

            await sut.SynchroniseApprentices(_cancellationToken);

            _apprenticeAccountsApiClientMock.VerifyAll();
        }

        [Test]
        public async Task AndLastJobAuditNotNull_ThenTheRequestForApprenticesContainsTheDate()
        {
            SetupEmptyMocks();

            var apprentices = new List<Apprentice>
            {
                new Apprentice { ApprenticeId = Guid.NewGuid() },
                new Apprentice { ApprenticeId = Guid.NewGuid() }
            };

            var apprenticeIds = apprentices.Select(a => a.ApprenticeId).ToArray();

            var lastJobAudit = new JobAudit()
            {
                JobName = nameof(SynchroniseApprenticeDetailsFunction),
                StartTime = DateTime.UtcNow.AddDays(-1)
            };

            _jobAuditRepositoryMock.Setup(x => x.GetMostRecentJobAudit(_cancellationToken)).ReturnsAsync(lastJobAudit);
            _apprenticeRepositoryMock.Setup(x => x.GetApprentices(_cancellationToken)).ReturnsAsync(apprentices);

            _apprenticeAccountsApiClientMock.Setup(x => x.SynchroniseApprentices(
                apprenticeIds,
                lastJobAudit.StartTime,
                _cancellationToken
            )).Verifiable();

            var sut = CreateService(
                _loggerMock.Object,
                _apprenticeAccountsApiClientMock.Object,
                _apprenticeRepositoryMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object
            );

            await sut.SynchroniseApprentices(_cancellationToken);

            _apprenticeAccountsApiClientMock.VerifyAll();
        }

        [Test]
        public async Task AndAuditQueryFails_ThenExceptionIsThrown()
        {
            var apprentices = new List<Apprentice>
            {
                new Apprentice { ApprenticeId = Guid.NewGuid() },
                new Apprentice { ApprenticeId = Guid.NewGuid() }
            };

            SetupEmptyMocks();

            _apprenticeRepositoryMock.Setup(x => x.GetApprentices(_cancellationToken)).ReturnsAsync(apprentices);

            var sut = CreateService(
                _loggerMock.Object,
                _apprenticeAccountsApiClientMock.Object,
                _apprenticeRepositoryMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object
            );

            _jobAuditRepositoryMock
                .Setup(x => x.GetMostRecentJobAudit(_cancellationToken))
                .ThrowsAsync(new Exception(nameof(Exception)));

            var result = await sut.SynchroniseApprentices(_cancellationToken);

            _jobAuditRepositoryMock.Verify(x => x.RecordAudit(_cancellationToken, It.IsAny<JobAudit>()), Times.Exactly(1));

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task AndApiResponseIsNull_ThenLogsAuditAndReturnsDefault()
        {
            SetupEmptyMocks();

            var apprentices = new List<Apprentice>
            {
                new Apprentice { ApprenticeId = Guid.NewGuid() },
                new Apprentice { ApprenticeId = Guid.NewGuid() }
            };

            _apprenticeRepositoryMock.Setup(x => x.GetApprentices(_cancellationToken)).ReturnsAsync(apprentices);

            Response<ApprenticeSyncResponseDto>? response = null;

            _apprenticeAccountsApiClientMock.Setup(x => x.SynchroniseApprentices(
                It.IsAny<Guid[]>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(response);

            var sut = CreateService(
                _loggerMock.Object,
                _apprenticeAccountsApiClientMock.Object,
                _apprenticeRepositoryMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object
            );

            var result = await sut.SynchroniseApprentices(_cancellationToken);

            _jobAuditRepositoryMock.Verify(x => x.RecordAudit(_cancellationToken, It.IsAny<JobAudit>()), Times.Exactly(1));

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task AndApiResponseIsEmpty_ThenLogsAuditAndReturnsDefault()
        {
            SetupEmptyMocks();

            var apprentices = new List<Apprentice>
            {
                new Apprentice { ApprenticeId = Guid.NewGuid() },
                new Apprentice { ApprenticeId = Guid.NewGuid() }
            };

            _apprenticeRepositoryMock.Setup(x => x.GetApprentices(_cancellationToken)).ReturnsAsync(apprentices);

            var response = new Response<ApprenticeSyncResponseDto>("{ \"apprentices\":[]}", new HttpResponseMessage(HttpStatusCode.OK), () => new ApprenticeSyncResponseDto() { Apprentices = [] });

            var apprenticeIds = apprentices.Select(a => a.ApprenticeId).ToArray();

            _apprenticeAccountsApiClientMock.Setup(x => x.SynchroniseApprentices(
                apprenticeIds,
                null,
                _cancellationToken
            )).ReturnsAsync(response);

            var sut = CreateService(
                _loggerMock.Object,
                _apprenticeAccountsApiClientMock.Object,
                _apprenticeRepositoryMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object
            );

            var result = await sut.SynchroniseApprentices(_cancellationToken);

            _jobAuditRepositoryMock.Verify(x => x.RecordAudit(_cancellationToken, It.IsAny<JobAudit>()), Times.Exactly(1));

            Assert.That(result, Is.EqualTo(0));
        }

        private void SetupEmptyMocks()
        {
            _apprenticeRepositoryMock = new Mock<IApprenticeRepository>();
            _jobAuditRepositoryMock = new Mock<IJobAuditRepository>();
            _memberRepositoryMock = new Mock<IMemberRepository>();
            _apprenticeAccountsApiClientMock = new Mock<IApprenticeAccountsApiClient>();
        }
    }
}
