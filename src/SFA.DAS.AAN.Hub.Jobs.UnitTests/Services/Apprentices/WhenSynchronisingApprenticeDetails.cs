using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using RestEase;
using SFA.DAS.AAN.Hub.Data;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Api.Response;
using SFA.DAS.AAN.Hub.Jobs.Functions;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.Testing.AutoFixture;
using System.Net;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services.Apprentices
{
    public class WhenSynchronisingApprenticeDetails
    {
        private Mock<IJobAuditRepository> _jobAuditRepositoryMock = null!;
        private Mock<IMemberRepository> _memberRepositoryMock = null!;
        private Mock<IApprenticeAccountsApiClient> _apprenticeAccountsApiClientMock = null!;
        private Mock<ILogger<SynchroniseApprenticeDetailsService>> _loggerMock = new();
        private Mock<IAanDataContext> _aanDataContextMock = null!;
        private Mock<ISynchroniseApprenticeDetailsRepository> _synchroniseApprenticeDetailsRepository = null!;
        private CancellationToken _cancellationToken;

        private Response<ApprenticeSyncResponseDto> emptyResponse = 
            new Response<ApprenticeSyncResponseDto>(
                "{ \"apprentices\":[]}", 
                new HttpResponseMessage(HttpStatusCode.OK), 
                () => new ApprenticeSyncResponseDto() { Apprentices = [] }
        );

        private Response<ApprenticeSyncResponseDto> badResponse =
            new Response<ApprenticeSyncResponseDto>(
                "{ \"apprentices\":[]}",
                new HttpResponseMessage(HttpStatusCode.BadRequest),
                () => new ApprenticeSyncResponseDto()
                {
                    Apprentices = []
                }
        );

        [SetUp]
        public void Init()
        {
            Fixture fixture = new();
            fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _cancellationToken = fixture.Create<CancellationToken>();
            _loggerMock = new Mock<ILogger<SynchroniseApprenticeDetailsService>>();
        }

        private static SynchroniseApprenticeDetailsService CreateService(
            IApprenticeAccountsApiClient apprenticeAccountsApiClientMock,
            IAanDataContext aanDataContext,
            IJobAuditRepository jobAuditRepository,
            IMemberRepository memberRepository,
            ISynchroniseApprenticeDetailsRepository synchroniseApprenticeDetailsRepository
        )
        {
            return new SynchroniseApprenticeDetailsService(apprenticeAccountsApiClientMock, jobAuditRepository, memberRepository, aanDataContext, synchroniseApprenticeDetailsRepository);
        }

        [Test]
        public async Task AndMembersEmpty_ThenLogsAuditAndReturnsDefault()
        {
            SetupEmptyMocks();

            _memberRepositoryMock.Setup(a => a.GetActiveApprenticeMembers(_cancellationToken)).ReturnsAsync(new List<Member>());

            var sut = CreateService(
                _apprenticeAccountsApiClientMock.Object,
                _aanDataContextMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object,
                _synchroniseApprenticeDetailsRepository.Object
            );

            var result = await sut.SynchroniseApprentices(_cancellationToken);

            _synchroniseApprenticeDetailsRepository.Verify(x => 
                x.AddJobAudit(It.IsAny<JobAudit>(), It.IsAny<string>(), _cancellationToken), Times.Exactly(1));

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        [RecursiveMoqAutoData]
        public async Task AndGetApprenticesByIdReturnsEmpty_ThenLogsAuditAndReturnsDefault(
            List<Member> members
        )
        {
            SetupEmptyMocks();

            _memberRepositoryMock.Setup(a => a.GetActiveApprenticeMembers(_cancellationToken)).ReturnsAsync(members);

            var apprenticeIds = members.Select(a => a.Apprentice == null ? new Guid() : a.Apprentice.ApprenticeId).ToArray();

            _apprenticeAccountsApiClientMock.Setup(x => x.SynchroniseApprentices(
                apprenticeIds,
                null,
                _cancellationToken
            )).ReturnsAsync(badResponse);

            var sut = CreateService(
                _apprenticeAccountsApiClientMock.Object,
                _aanDataContextMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object,
                _synchroniseApprenticeDetailsRepository.Object
            );

            var result = await sut.SynchroniseApprentices(_cancellationToken);

            Assert.That(result, Is.EqualTo(0));

            _apprenticeAccountsApiClientMock.Verify();
        }

        [Test]
        [RecursiveMoqAutoData]
        public async Task AndLastJobAuditNull_ThenTheRequestForApprenticesContainsNoDate(
            List<Member> members
        )
        {
            SetupEmptyMocks();

            var apprenticeIds = members.Select(a => a.Apprentice == null ? new Guid() : a.Apprentice.ApprenticeId).ToArray();

            JobAudit? lastJobAudit = null;

            _memberRepositoryMock.Setup(a => a.GetActiveApprenticeMembers(_cancellationToken)).ReturnsAsync(members);

            _jobAuditRepositoryMock.Setup(x => x.GetMostRecentJobAudit("SynchroniseApprenticeDetailsService", _cancellationToken))
                .ReturnsAsync(lastJobAudit);

            _apprenticeAccountsApiClientMock.Setup(x => x.SynchroniseApprentices(
                apprenticeIds,
                null,
                _cancellationToken
            )).ReturnsAsync(emptyResponse).Verifiable();

            var sut = CreateService(
                _apprenticeAccountsApiClientMock.Object,
                _aanDataContextMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object,
                _synchroniseApprenticeDetailsRepository.Object
            );

            await sut.SynchroniseApprentices(_cancellationToken);

            _apprenticeAccountsApiClientMock.VerifyAll();
        }

        [Test]
        [RecursiveMoqAutoData]
        public async Task AndApprenticeSyncDtosReturned_ThenMembersAreUpdated(
            List<Member> members
        )
        {
            SetupEmptyMocks();

            var apprenticeIds = members.Select(a => a.Apprentice == null ? new Guid() : a.Apprentice.ApprenticeId).ToArray();

            JobAudit? lastJobAudit = null;

            _memberRepositoryMock.Setup(a => a.GetActiveApprenticeMembers(_cancellationToken)).ReturnsAsync(members);

            _jobAuditRepositoryMock.Setup(x => x.GetMostRecentJobAudit("SynchroniseApprenticeDetailsService", _cancellationToken))
                .ReturnsAsync(lastJobAudit);

            var response = new Response<ApprenticeSyncResponseDto>(
                "{ \"apprentices\":[]}",
                new HttpResponseMessage(HttpStatusCode.OK),
                () => new ApprenticeSyncResponseDto()
                {
                    Apprentices = members.Select(a => 
                        new ApprenticeSyncDto() { 
                            ApprenticeID = a.Apprentice?.ApprenticeId ?? Guid.NewGuid(),
                            FirstName = a.FirstName,
                            LastName = a.LastName,
                            Email = a.Email
                        }
                    ).ToArray()
                }
            );

            _apprenticeAccountsApiClientMock.Setup(x => x.SynchroniseApprentices(
                apprenticeIds,
                null,
                _cancellationToken
            )).ReturnsAsync(response).Verifiable();

            _synchroniseApprenticeDetailsRepository.Setup(a =>
                a.UpdateMemberDetails(It.IsAny<Member>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>())).Verifiable();

            var sut = CreateService(
                _apprenticeAccountsApiClientMock.Object,
                _aanDataContextMock.Object,
                _jobAuditRepositoryMock.Object,
                _memberRepositoryMock.Object,
                _synchroniseApprenticeDetailsRepository.Object
            );

            var result = await sut.SynchroniseApprentices(_cancellationToken);

            _synchroniseApprenticeDetailsRepository.Verify(a => 
                a.UpdateMemberDetails(
                    It.IsAny<Member>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>()
                    ), 
                Times.Exactly(3)
            );

            Assert.That(result, Is.EqualTo(3));
        }

        private void SetupEmptyMocks()
        {
            _jobAuditRepositoryMock = new Mock<IJobAuditRepository>();
            _memberRepositoryMock = new Mock<IMemberRepository>();
            _apprenticeAccountsApiClientMock = new Mock<IApprenticeAccountsApiClient>();
            _synchroniseApprenticeDetailsRepository = new Mock<ISynchroniseApprenticeDetailsRepository>();
            _aanDataContextMock = new Mock<IAanDataContext>();
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            emptyResponse.Dispose();
            badResponse.Dispose();
        }
    }
}
