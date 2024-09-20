using AutoFixture;
using NUnit.Framework;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.IntegrationTests.DbContext;

namespace SFA.DAS.AAN.Hub.Jobs.IntegrationTests.EventSignUpNotification
{
    public class EventSignUpNotificationTests
    {
        private CancellationToken _cancellationToken = CancellationToken.None;
        private Fixture _fixture = null!;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Test]
        public async Task GetEventSignUpNotification_Returns_Notifications_For_Members_Receiving_Notifications()
        {
            var attendances = _fixture.Build<Attendance>()
                                      .With(a => a.AddedDate, DateTime.UtcNow.AddHours(-1)) 
                                      .With(a => a.CalendarEvent, _fixture.Build<CalendarEvent>()
                                                                          .With(ce => ce.StartDate, DateTime.UtcNow.AddHours(-2)) 
                                                                          .With(ce => ce.Member, _fixture.Build<Member>()
                                                                                                            .With(m => m.ReceiveNotifications, true)
                                                                                                            .Create())
                                                                          .Create())
                                      .CreateMany(1)
                                      .ToList();

            using (var context = InMemoryAanDataContext.CreateInMemoryContext($"{nameof(GetEventSignUpNotification_Returns_Notifications_For_Members_Receiving_Notifications)}_InMemoryContext"))
            {
                await context.AddRangeAsync(attendances);
                await context.SaveChangesAsync(_cancellationToken);

                var sut = new EventSignUpNotificationRepository(context);
                var result = await sut.GetEventSignUpNotification();

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Has.Count.EqualTo(attendances.Count));

                foreach (var notification in result)
                {
                    var correspondingAttendance = attendances.FirstOrDefault(a => a.CalendarEvent.Id == notification.CalendarEventId);
                    Assert.That(notification.CalendarEventId, Is.EqualTo(correspondingAttendance?.CalendarEvent.Id));
                    Assert.That(notification.CalendarName, Is.EqualTo(correspondingAttendance?.CalendarEvent.Calender.CalendarName));
                    Assert.That(notification.FirstName, Is.EqualTo(correspondingAttendance?.CalendarEvent.Member.FirstName));
                    Assert.That(notification.LastName, Is.EqualTo(correspondingAttendance?.CalendarEvent.Member.LastName));
                    Assert.That(notification.NewAmbassadorsCount, Is.EqualTo(attendances.Count(a => a.CalendarEventId == correspondingAttendance?.CalendarEvent.Id && a.AddedDate >= DateTime.UtcNow.AddHours(-24))));
                    Assert.That(notification.TotalAmbassadorsCount, Is.EqualTo(attendances.Count(a => a.CalendarEventId == correspondingAttendance?.CalendarEvent.Id)));
                }
            }
        }

        [Test]
        public async Task GetEventSignUpNotification_Returns_Empty_When_No_Notifications()
        {
            using (var context = InMemoryAanDataContext.CreateInMemoryContext($"{nameof(GetEventSignUpNotification_Returns_Empty_When_No_Notifications)}_InMemoryContext"))
            {
                var sut = new EventSignUpNotificationRepository(context);
                var result = await sut.GetEventSignUpNotification();

                Assert.That(result, Is.Empty);
            }
        }

        [Test]
        public async Task GetEventSignUpNotification_Does_Not_Return_Members_Not_Receiving_Notifications()
        {
            var attendances = _fixture.Build<Attendance>()
                                      .With(a => a.AddedDate, DateTime.UtcNow.AddHours(-1)) 
                                      .With(a => a.CalendarEvent, _fixture.Build<CalendarEvent>()
                                                                          .With(ce => ce.StartDate, DateTime.UtcNow.AddHours(-2)) 
                                                                          .With(ce => ce.Member, _fixture.Build<Member>()
                                                                                                            .With(m => m.ReceiveNotifications, false) 
                                                                                                            .Create())
                                                                          .Create())
                                      .CreateMany(3)
                                      .ToList();

            using (var context = InMemoryAanDataContext.CreateInMemoryContext($"{nameof(GetEventSignUpNotification_Does_Not_Return_Members_Not_Receiving_Notifications)}_InMemoryContext"))
            {
                await context.AddRangeAsync(attendances);
                await context.SaveChangesAsync(_cancellationToken);

                var sut = new EventSignUpNotificationRepository(context);
                var result = await sut.GetEventSignUpNotification();

                Assert.That(result, Is.Empty);
            }
        }
    }
}
