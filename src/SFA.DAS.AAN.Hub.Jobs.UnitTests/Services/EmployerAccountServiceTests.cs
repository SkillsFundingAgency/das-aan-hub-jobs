using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Services;
using SFA.DAS.Encoding;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Services;

[TestFixture]
public class EmployerAccountsServiceTests
{
    private Mock<IOuterApiClient> _mockOuterApiClient;
    private Mock<IEncodingService> _mockEncodingService;
    private Mock<ILogger<EmployerAccountsService>> _mockLogger;
    private EmployerAccountsService _service;

    [SetUp]
    public void Setup()
    {
        _mockOuterApiClient = new Mock<IOuterApiClient>();
        _mockEncodingService = new Mock<IEncodingService>();
        _mockLogger = new Mock<ILogger<EmployerAccountsService>>();
        _service = new EmployerAccountsService(_mockOuterApiClient.Object, _mockEncodingService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetEmployerUserAccounts_ShouldReturnEncodedEmployerAccountId()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var employerAccountId = 12345;
        var encodedAccountId = "mjkldn";

        var apiResponse = new GetMemberByIdQueryResult { EmployerAccountId = employerAccountId };

        _mockOuterApiClient
            .Setup(client => client.GetMemberById(memberId, CancellationToken.None))
            .ReturnsAsync(apiResponse);

        _mockEncodingService
            .Setup(encoder => encoder.Encode(employerAccountId, EncodingType.AccountId))
            .Returns(encodedAccountId);

        // Act
        var result = await _service.GetEmployerUserAccounts(memberId);

        // Assert
        result.Should().Be(encodedAccountId);

        _mockOuterApiClient.Verify(client => client.GetMemberById(memberId, CancellationToken.None), Times.Once);
        _mockEncodingService.Verify(encoder => encoder.Encode(employerAccountId, EncodingType.AccountId), Times.Once);
    }
}