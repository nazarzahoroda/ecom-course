using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Persistence.Identity;

namespace EcomCourse.IntegrationTests.Infrastructure
{
    [CollectionDefinition("IntegrationTests")]
    public class IntegrationTestDefinition
        : ICollectionFixture<
            CustomWebApplicationFactory<Program, EcomCourseDbContext, IdentityDbContext>
        > { }
}
