﻿using NUnit.Framework;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Repositories;
using SFA.DAS.AAN.Hub.Jobs.IntegrationTests.DbContext;

namespace SFA.DAS.AAN.Hub.Jobs.IntegrationTests.JobAudits
{
    public class JobAuditWithRecentStartDate
    {
        private CancellationToken cancellationToken = CancellationToken.None;

        [Test]
        public async Task Job_audit_with_todays_date_gets_returned()
        {
            JobAudit resultOne = new JobAudit() { JobName = nameof(JobAudit), StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddMinutes(5) };
            JobAudit resultTwo = new JobAudit() { JobName = nameof(JobAudit), StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(-1).AddMinutes(5) };

            JobAudit? result;

            using (var context = InMemoryAanDataContext.CreateInMemoryContext($"{nameof(Job_audit_with_todays_date_gets_returned)}_InMemoryContext"))
            {
                await context.JobAudits.AddRangeAsync([resultOne, resultTwo]);
                await context.SaveChangesAsync(cancellationToken);
                var sut = new JobAuditRepository(context);
                result = await sut.GetMostRecentJobAudit(nameof(JobAudit), cancellationToken);
            }

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.StartTime.Date, Is.EqualTo(DateTime.UtcNow.Date));
        }
    }
}