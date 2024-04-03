using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using SFA.DAS.AAN.Hub.Jobs.Api.Clients;
using SFA.DAS.AAN.Hub.Jobs.Api.Response;
using SFA.DAS.Testing.AutoFixture;
using System.Net;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Api;

public class ApprenticeAccountsApiTests
{
    private Mock<ILogger<ApprenticeAccountsApi>> _logger = new Mock<ILogger<ApprenticeAccountsApi>>();

    [Test]
    [MoqAutoData]
    public async Task AndDeserializationTypeMatch_ThenGetDeserializedResponseObjectIsSuccessful(Guid apprenticeId, DateTime temporaryDate)
    {
        var apprentice = new ApprenticeSyncDto() { FirstName = "FirstName", LastName = "LastName", ApprenticeID = apprenticeId, Email = "test@email.com", DateOfBirth = temporaryDate, LastUpdatedDate = temporaryDate };
        var Apprentices = new[] { apprentice };
        var apprenticeSyncResponseDto = new ApprenticeSyncResponseDto(Apprentices);
        var serializedResponse = JsonConvert.SerializeObject(apprenticeSyncResponseDto);

        var clientFactory = SetupClientFactory<ApprenticeSyncResponseDto>(serializedResponse);

        var sut = new ApprenticeAccountsApi(_logger.Object, clientFactory.Object);

        await sut.PostValueAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<object>());

        var response = sut.GetDeserializedResponseObject<ApprenticeSyncResponseDto>();

        var resultingApprentice = response.Apprentices.First();

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Apprentices.Count, Is.EqualTo(1));
        Assert.That(resultingApprentice, Is.EqualTo(apprentice));
    }

    [Test]
    public async Task AndDeserializationTypeMismatch_ThenGetDeserializedResponseObjectIsUnsuccessful()
    {
        var serializedResponse = JsonConvert.SerializeObject(new ApprenticeSyncResponseDto());

        var clientFactory = SetupClientFactory<ApprenticeSyncResponseDto>(serializedResponse);

        var sut = new ApprenticeAccountsApi(_logger.Object, clientFactory.Object);

        await sut.PostValueAsync(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<object>());

        Assert.Throws<JsonSerializationException>(() => sut.GetDeserializedResponseObject<string[]>());
    }

    private Mock<IHttpClientFactory> SetupClientFactory<T>(string data) where T : class
    {
        var _clientFactory = new Mock<IHttpClientFactory>();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(data)
                }
            );

        var client = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("http://localhost") };
        _clientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(client);

        return _clientFactory;
    }
}
