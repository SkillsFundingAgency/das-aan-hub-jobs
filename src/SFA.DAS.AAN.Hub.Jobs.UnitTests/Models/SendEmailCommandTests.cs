using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Jobs.Models;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.Models;

public class SendEmailCommandTests
{
    public record TestEmailTemplate(string Name, string Id);

    private TestEmailTemplate _template = null!;
    private Notification _notification = null!;
    private SendEmailCommand _sut = null!;

    [SetUp]
    public void Init()
    {
        Fixture fixture = new();
        _template = fixture.Create<TestEmailTemplate>();
        _notification = fixture.Build<Notification>().With(n => n.Tokens, JsonSerializer.Serialize(_template)).Create();
        _sut = new SendEmailCommand(_notification);
    }

    [Test]
    public void ThenPopulatesRecipientAddressFromNotificationMemberEmail() =>
        _sut.RecipientsAddress.Should().Be(_notification.Member.Email);

    [Test]
    public void ThenPopulatesTemplateIdFromNotificationTemplateName() =>
        _sut.TemplateId.Should().Be(_notification.TemplateName);

    [Test]
    public void ThenPopulatesTokensFromNotificationTokens()
    {
        using AssertionScope scope = new();
        _sut.Tokens.Should().HaveCount(2);
        _sut.Tokens[nameof(TestEmailTemplate.Id)].Should().Be(_template.Id);
        _sut.Tokens[nameof(TestEmailTemplate.Name)].Should().Be(_template.Name);
    }
}
