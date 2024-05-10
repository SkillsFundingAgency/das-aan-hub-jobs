using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data;

namespace SFA.DAS.AAN.Hub.Jobs.IntegrationTests.DbContext
{
    public static class InMemoryAanDataContext
    {
        public static AanDataContext CreateInMemoryContext(string contextName)
        {
            var _dbContextOptions = new DbContextOptionsBuilder<AanDataContext>()
                .UseInMemoryDatabase(databaseName: contextName)
                .Options;

            return new AanDataContext(_dbContextOptions);
        }
    }
}
