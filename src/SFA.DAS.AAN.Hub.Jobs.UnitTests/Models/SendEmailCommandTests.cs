using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Jobs.Configuration;
using SFA.DAS.AAN.Hub.Jobs.Models;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Models;

public class SendEmailCommandTests
{
    public record TestTokens(string Name, string Id);

    private TestTokens _testTokens = null!;
    private Notification _notification = null!;
    private SendEmailCommand _sut = null!;
    private ApplicationConfiguration _applicationConfiguration = null!;

    [SetUp]
    public void Init()
    {
        Fixture fixture = new();
        _testTokens = fixture.Create<TestTokens>();
        _applicationConfiguration = fixture.Create<ApplicationConfiguration>();
        _notification = fixture
            .Build<Notification>()
            .With(n => n.Tokens, JsonSerializer.Serialize(_testTokens))
            .With(n => n.TemplateName, _applicationConfiguration.Notifications.Templates.First().Key)
            .Create();
        _sut = new SendEmailCommand(_notification, _applicationConfiguration);
    }

    [Test]
    public void ThenPopulatesRecipientAddressFromNotificationMemberEmail() =>
        _sut.RecipientsAddress.Should().Be(_notification.Member.Email);

    [Test]
    public void ThenPopulatesTemplateIdUsingNotificationTemplateNameAndConfiguration() =>
        _sut.TemplateId.Should().Be(_applicationConfiguration.Notifications.Templates.First().Value);

    [Test]
    public void ThenPopulatesTokensFromNotificationTokens()
    {
        using AssertionScope scope = new();
        _sut.Tokens.Should().HaveCount(2);
        _sut.Tokens[nameof(TestTokens.Id)].Should().Be(_testTokens.Id);
        _sut.Tokens[nameof(TestTokens.Name)].Should().Be(_testTokens.Name);
    }
}
