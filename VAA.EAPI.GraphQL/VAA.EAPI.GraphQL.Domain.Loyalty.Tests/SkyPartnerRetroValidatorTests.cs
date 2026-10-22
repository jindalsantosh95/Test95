using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class SkyPartnerRetroValidatorTests
    {
        public SkyPartnerRetroValidatorTests()
        {
            Setup.CreateTestEnvironmentSettings();
        }

        [Theory]
        [InlineData("-190", -190, false)]
        [InlineData("-190", -189, true)]
        [InlineData("-191", -190, true)]
        public void ValidateFlightActivityDate_UsesRetroFlightActivityNoOfDaysOverride(
            string configuredLimit,
            int flightActivityDateOffset,
            bool isValid)
        {
            // Arrange
            ConfigReader.Set(Common.Constants.Environment.RetroFlightActivityNoOfDays, configuredLimit);
            var request = TestDataProvider.GetTestData<SkyPartnerRetroRequest>("Loyalty//SkyPartnerRetroRequest");
            request.LoyTransaction.BookingDetails.FlightDetails.FlightActivityDate = DateTime.Today.AddDays(flightActivityDateOffset);
            var validator = new SkyPartnerRetroValidator(HttpMethod.Post);

            // Act
            var result = validator.Validate(request);

            // Assert
            Assert.Equal(isValid, result.IsValid);
        }
    }
}
