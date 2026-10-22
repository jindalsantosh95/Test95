using VAA.EAPI.Common.Helpers;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Tests
{
    public class DepartureDateMapperTests
    {
        [Fact]
        public void DepartureDate_TestCase_1()
        {
            //Arrange
            string departureDate = "30NOV23";
            string departureTime = "1020A";

            //Act
            var result = DateTimeHelper.Air4PNRDateTimeConversion(departureDate, departureTime);

            //Assert
            Assert.True(result.Value.Day == 30);
            Assert.True(result.Value.TimeOfDay.Hours == 10);
        }

        [Fact]
        public void DepartureDate_TestCase_2()
        {
            //Arrange
            string departureDate = "13JAN24";
            string departureTime = "715P";

            //Act
            var result = DateTimeHelper.Air4PNRDateTimeConversion(departureDate, departureTime);

            //Assert
            Assert.True(result.Value.Day == 13);
            Assert.True(result.Value.TimeOfDay.Hours == 19);
        }

        [Fact]
        public void DepartureDate_TestCase_3()
        {
            //Arrange
            string departureDate = "21DEC23";
            string departureTime = "823A";

            //Act
            var result = DateTimeHelper.Air4PNRDateTimeConversion(departureDate, departureTime);

            //Assert
            Assert.True(result.Value.Day == 21);
            Assert.True(result.Value.TimeOfDay.Hours == 08);
        }

        [Fact]
        public void DepartureDate_TestCase_4()
        {
            //Arrange
            string departureDate = "17FEB24";
            string departureTime = "1855P";

            //Act
            var result = DateTimeHelper.Air4PNRDateTimeConversion(departureDate, departureTime);

            //Assert
            Assert.True(result.Value.Day == 17);
            Assert.True(result.Value.TimeOfDay.Hours == 18);
        }
    }
}
