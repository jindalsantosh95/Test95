using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services.EnrolmentCustomer;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.Account
{
    public class CustomerSearchServiceTests 
    {
        private readonly ICustomerProfileService _customerProfileService;
        private readonly Mock<IRequestHandler<CosmosReaderServiceQuery, List<object>>> _cosmosReaderServiceQueryHandler;

        public CustomerSearchServiceTests()
        {
            _cosmosReaderServiceQueryHandler = new Mock<IRequestHandler<CosmosReaderServiceQuery, List<object>>>();
            _customerProfileService = new CustomerProfileService(_cosmosReaderServiceQueryHandler.Object);

            SettingsHelper.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task GetCustomerProfileDetails_withCustomerId_success()
        {
            var request = new CustomerProfileRequest { CustomerId = "D47166A3-533D-9159-E053-6941D90A1989" };
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerSearch\\ListOfCustomerSearchDocs");

            _cosmosReaderServiceQueryHandler.Setup(m => m.Handle(It.IsAny<CosmosReaderServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);
            
            // Act
            var result = await _customerProfileService.GetCustomerDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CustomerSearch", result.ToList().First().DocType);
            Assert.Equal("D47166A3-533D-9159-E053-6941D90A1989", result.ToList().First().CustomerId);
            Assert.Equal("Gosslin", result.ToList().First().FirstName);
            Assert.Equal("Jamesaa", result.ToList().First().LastName);
        }

        [Fact]
        public async Task GetCustomerProfileDetails_withFCNumber_success()
        {
            var request = new CustomerProfileRequest { MemberNumber = "1074714856" };
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerSearch\\ListOfCustomerSearchDocs");

            _cosmosReaderServiceQueryHandler.Setup(m => m.Handle(It.IsAny<CosmosReaderServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);

            // Act
            var result = await _customerProfileService.GetCustomerDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CustomerSearch", result.ToList().First().DocType);
            Assert.Equal("Gosslin", result.ToList().First().FirstName);
            Assert.Equal("Jamesaa", result.ToList().First().LastName);
            Assert.Equal("1074714856", result.ToList().First().LoyaltyMemberNumber);
        }

        [Fact]
        public async Task GetCustomerProfileDetails_withEmail_success()
        {
            var request = new CustomerProfileRequest { Email = "test222555@gmail.com" };
            var mockResponse = TestDataProvider.GetTestData<List<object>>("CustomerSearch\\ListOfCustomerSearchDocs");

            _cosmosReaderServiceQueryHandler.Setup(m => m.Handle(It.IsAny<CosmosReaderServiceQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockResponse);

            // Act
            var result = await _customerProfileService.GetCustomerDetails(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CustomerSearch", result.ToList().First().DocType);
            Assert.Equal("D47166A3-533D-9159-E053-6941D90A1989", result.ToList().First().CustomerId);
            Assert.Equal("1074714856", result.ToList().First().LoyaltyMemberNumber);
        }
    }
}
