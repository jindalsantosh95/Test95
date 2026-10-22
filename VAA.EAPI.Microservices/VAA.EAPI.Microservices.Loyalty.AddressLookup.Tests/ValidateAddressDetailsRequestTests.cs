using System.Linq;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Query;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Validation;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public class ValidateAddressDetailsRequestTests
    {
        [Fact]
        public void ValidateAddressDetailsRequest_ShouldReturn10149_ForNullPostCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQuery>("AddressLookUpMS\\AddressDetailsRequest");
            request.PostCode = null;

            //Act
            var result = ValidateAddressDetailsRequest.AddressDetailsRequestValidator(request);

            //Assert
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10149", result.Value.Errors.First().Code);
        }

        [Fact]
        public void ValidateAddressDetailsRequest_ShouldReturnG018_ForNullCountryCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQuery>("AddressLookUpMS\\AddressDetailsRequest");
            request.CountryCode = null;

            //Act
            var result = ValidateAddressDetailsRequest.AddressDetailsRequestValidator(request);

            //Assert
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("G018", result.Value.Errors.First().Code);
        }

        [Fact]
        public void ValidateAddressDetailsRequest_ShouldReturn10150_ForInvalidCountryCode()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQuery>("AddressLookUpMS\\AddressDetailsRequest");
            request.CountryCode = "TestCountryCode";

            //Act
            var result = ValidateAddressDetailsRequest.AddressDetailsRequestValidator(request);

            //Assert
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("10150", result.Value.Errors.First().Code);
        }

        [Fact]
        public void ValidateAddressDetailsRequest_ShouldReturnG016_ForNullAddressLine1()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<AddressLookupQuery>("AddressLookUpMS\\AddressDetailsRequest");
            request.AddressLine1 = null;

            //Act
            var result = ValidateAddressDetailsRequest.AddressDetailsRequestValidator(request);

            //Assert
            Assert.NotNull(result.Value.Errors);
            Assert.Equal("G016", result.Value.Errors.First().Code);
        }
    }
}
