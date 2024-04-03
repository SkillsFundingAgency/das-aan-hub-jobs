using AutoFixture.Kernel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SFA.DAS.AAN.Hub.Jobs.UnitTests.FixtureSpecimens
{
    [ExcludeFromCodeCoverage]
    public class GuidSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var pi = request as PropertyInfo;
            if (pi != null && pi.PropertyType == typeof(Guid))
            {
                return Guid.NewGuid();
            }
            return new NoSpecimen();
        }
    }
}
