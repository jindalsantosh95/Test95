using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command.GbgService;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    
    public class AddressLookUpServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly Mock<ILogger<GbgService>> _gbgLogger;
        private readonly HttpConnector _httpConnector;
        private readonly AddressLookupService addressLookUpService;
        private readonly  IGbgService _gbgService;


        public AddressLookUpServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _gbgLogger = new Mock<ILogger<GbgService>>();
            _gbgService = new GbgService(_httpConnector,_gbgLogger.Object);
            addressLookUpService = new AddressLookupService(_gbgService);
            AddressLookupTestsCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task AddressLookUpService_ShouldReturnErrorResponse_GetAddressDetails()
        {
            //Arrange

            var result =  await addressLookUpService.GetAllAddresses("", "RH101DJ","219");

            //Assert
            //Assert
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Equal("G018", result.Errors.First().Code);
            Assert.Contains("One of the mandatory field is missing in the request", result.Errors.First().Message);
        }
        [Fact]
        public async Task AddressLookUpService_ShouldReturnErrorResponse_InvalidCountryCodeGetAddressDetails()
        {
            //Arrange

            var result = await addressLookUpService.GetAllAddresses("Test", "RH101DJ", "219");

            //Assert
            //Assert
            Assert.Equal(Status.BusinessException, result.Status);
            Assert.Equal("10150", result.Errors.First().Code);
            Assert.Contains("Country is not valid for address lookup", result.Errors.First().Message);          

        }

        [Fact]
        public async Task AddressLookUpService_ShouldReturnSuccessREsponse_FindAddressDetails()
        {
            //Arrange

            var result = await addressLookUpService.FindAddresses("219 RH101DJ", null);

            //Assert
            Assert.IsType<Response<IEnumerable<AddressDetail>>>(result);
            Assert.NotNull(result.Value?.First().AddressLine1);
            Assert.NotNull(result.Value.First().AddressId);
            Assert.Equal("219 Platform 11 The Boulevard Crawley RH10 1DJ", result.Value.First().AddressLine1);
        }

        [Fact]
        public async Task AddressLookUpService_ShouldReturnSuccessREsponse_FindAddressDetails_With_CountryCode()
        {
            //Arrange

            var result = await addressLookUpService.FindAddresses("219 RH101DJ", "GB");

            //Assert
            Assert.IsType<Response<IEnumerable<AddressDetail>>>(result);
            Assert.NotNull(result.Value?.First().AddressLine1);
            Assert.NotNull(result.Value.First().AddressId);
            Assert.Equal("219 Platform 11 The Boulevard Crawley RH10 1DJ", result.Value.First().AddressLine1);
        }

        [Fact]
        public async Task AddressLookUpService_ShouldReturnSuccessREsponse_RetreiveAddressDetails()
        {
            //Arrange

            var result = await addressLookUpService.RetrieveAddress("GB|RM|B|54367428|ENG");

            //Assert
            Assert.IsType<Response<IEnumerable<AddressDetail>>>(result);
            Assert.Equal("219 Platform", result.Value.First().AddressLine1);
            Assert.Equal("RH10 1DJ", result.Value.First().PostalCode);
            Assert.Equal("United Kingdom", result.Value.First().CountryName);
            Assert.Equal("GB", result.Value.First().CountryCode);
        }
    }
}
