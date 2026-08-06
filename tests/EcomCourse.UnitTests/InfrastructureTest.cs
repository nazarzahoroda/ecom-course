using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EcomCourse.Infrastructure;

namespace EcomCourse.UnitTests
{
    public class InfrastructureTest
    {
        // тест для перевірки на білд DI
        [Fact]
        public void InfrastructureRegistration()
        {
            var service = new ServiceCollection();

            var exception = Record.Exception(()
                => service.AddInfrastructure(new ConfigurationBuilder().Build())
            );

            Assert.Null(exception);
        }
    }
}
