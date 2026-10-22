using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Insurance.Domain;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Common;
using VAA.EAPI.Core.Functions;

namespace VAA.EAPI.Microservices.Retail.Insurance.Tests
{
    public class InsuranceQuoteRequestMapperTests
    {
        [Fact]
        public void MapQuoteRequest_ShouldMapBasicProperties()
        {
            // Arrange
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            var request = TestDataProvider.GetTestData<InsuranceQuoteRequest>("Insurance\\QuoteRequest");

            // Act
            var result = AllianzQuoteRequestMapper.MapQuoteRequest(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Credentials);
            Assert.NotNull(result.Gadget);
            Assert.NotNull(result.Customers);
            Assert.NotNull(result.Partner);
            Assert.NotNull(result.RequestDetail);
            Assert.NotNull(result.Travel);
        }

        [Fact]
        public void MapQuoteRequest_ShouldMapGadgetCorrectly()
        {
            // Arrange
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            var insuranceQuoteRequest = TestDataProvider.GetTestData<InsuranceQuoteRequest>("Insurance\\QuoteRequest");

            // Act
            var result = AllianzQuoteRequestMapper.MapQuoteRequest(insuranceQuoteRequest);

            // Assert
            Assert.Equal(insuranceQuoteRequest.Gadget.DeviceCategory, result.Gadget.DeviceCategory);
            Assert.Equal(insuranceQuoteRequest.Gadget.DeviceType, result.Gadget.DeviceType);
            Assert.Equal(insuranceQuoteRequest.Gadget.IpAddress, result.Gadget.IpAddress);
            Assert.Equal(insuranceQuoteRequest.Gadget.RequestingPlatform, result.Gadget.RequestingPlatform);
        }

        [Fact]
        public void MapQuoteRequest_ShouldMapCustomersCorrectly()
        {
            // Arrange
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            var insuranceQuoteRequest = TestDataProvider.GetTestData<InsuranceQuoteRequest>("Insurance\\QuoteRequest");

            // Act
            var result = AllianzQuoteRequestMapper.MapQuoteRequest(insuranceQuoteRequest);

            // Assert
            foreach (var expectedCustomer in result.Customers)
            {
                var actualCustomer = insuranceQuoteRequest.Customers.First(c => c.CustomerId == expectedCustomer.CustomerId);
                Assert.NotNull(actualCustomer);
                Assert.Equal(expectedCustomer.CustomerType, actualCustomer.CustomerType);
                Assert.Equal(expectedCustomer.FirstName, actualCustomer.FirstName);
                Assert.Equal(expectedCustomer.LastName, actualCustomer.LastName);
                Assert.Equal(expectedCustomer.Age, actualCustomer.Age);
            }

        }

        [Fact]
        public void MapQuoteRequest_ShouldMapTravelCorrectly()
        {
            // Arrange
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            var insuranceQuoteRequest = TestDataProvider.GetTestData<InsuranceQuoteRequest>("Insurance\\QuoteRequest");

            // Act
            var result = AllianzQuoteRequestMapper.MapQuoteRequest(insuranceQuoteRequest);

            // Assert
            foreach (var expectedSegment in result.Travel.TravelSegments)
            {
                var actualSegment = insuranceQuoteRequest.Travel.TravelSegments.FirstOrDefault(s => s.RouteGroup == expectedSegment.RouteGroup);
                Assert.NotNull(actualSegment);
                foreach (var expectedLeg in expectedSegment.TravelLegs)
                {
                    var actualLeg = actualSegment.TravelLegs.FirstOrDefault(l => l.RouteNumber == expectedLeg.RouteNumber);
                    Assert.NotNull(actualLeg);
                    Assert.Equal(expectedLeg.ClassOfService, actualLeg.ClassOfService);
                    Assert.Equal(expectedLeg.FareClass, actualLeg.FareClass);
                    Assert.Equal(expectedLeg.Sequence, actualLeg.Sequence);
                }
            }
        }

        [Fact]
        public void MapQuoteRequest_ShouldMapTravelDetailsCorrectly()
        {
            // Arrange        
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            var insuranceQuoteRequest = TestDataProvider.GetTestData<InsuranceQuoteRequest>("Insurance\\QuoteRequest");

            // Act
            var result = AllianzQuoteRequestMapper.MapQuoteRequest(insuranceQuoteRequest);

            // Assert
            Assert.Equal(insuranceQuoteRequest.Travel.TotalCost, result.Travel.TotalCost);
            Assert.Equal(insuranceQuoteRequest.Travel.ClassOfService, result.Travel.ClassOfService);
            Assert.Equal(insuranceQuoteRequest.Travel.MembershipStatus, result.Travel.MembershipStatus);

        }
    }
}
