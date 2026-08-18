using Xunit;

namespace EcomCourse.UnitTests.Domain.Primitives
{
    public class ValueObjectTests
    {

        [Fact]
        public void EqualsShouldReturnTrueWhenValuesAreSame()
        {

            var address1 = new Address("Львів", "Площа Ринок");
            var address2 = new Address("Львів", "Площа Ринок");

            bool result = address1.Equals(address2);

            Assert.True(result);
        }



        [Fact]
        public void EqualsShouldReturnFalseWhenValuesAreDifferent()
        {
            var address1 = new Address("Львів", "Площа Ринок");
            var address2 = new Address("Київ", "Хрещатик");


            bool result = address1.Equals(address2);


            Assert.False(result);
        }

    }
}
