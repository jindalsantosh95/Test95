using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Location;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Validations.OrderChange;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using OfferItemModel = VAA.EAPI.Domain.Virgin.Models.Offer.OfferItem;

namespace VAA.EAPI.NDC.Order.Tests.Common.OrderChangeRequestValidations
{
    public class OrderChangeValidationsTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;

        public OrderChangeValidationsTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _serviceProvider = new DefaultServiceProvider();
            _mediator = new Mediator(_serviceProvider);
        }

        [Fact]
        public void OrderChangeFOPValidations()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeSaleSeatReq");
            var orderDoc = TestDataProvider.GetTestData<OrderRS>("NDCOrder\\OrderChange\\OrderDocSaleSeat");

            var validationErrors = OrderChangeValidations.ValidateOrderChangeFormOfPayment<IATA_OrderViewRS>(request, orderDoc, _hostEnvironment, false);

            Assert.NotNull(validationErrors.ResponseMessages);
            Assert.Equal("759", validationErrors.ResponseMessages.Errors.First().Code);
        }

        [Fact]
        public void PostSaleCityCodeValidation()
        {
            var citycode = new CityCodeData()
            {
                CountryName = "Norway",
                CityCode = "",
                CountryCode = "NO",
                NDCCityCodeLocation = ""
            };

            var validationErrors = OrderChangeValidations.ValidateCityCode<IATA_OrderViewRS>(citycode, _hostEnvironment);

            Assert.NotNull(validationErrors.ResponseMessages);
            Assert.Equal("911", validationErrors.ResponseMessages.Errors.First().Code);
        }

        [Fact]
        public void ValidatePaymentFunctions_WhenPaymentFunctionsMissing_ForEvenExchange_Returns912_45()
        {
            // Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            request.Request.PaymentFunctions = null;

            var offerDoc = new OfferResponse
            {
                Offers = new List<OfferDetails>
                {
                    new()
                    {
                        OfferItems = new List<OfferItemModel>
                        {
                            new()
                            {
                                OfferitemPricing = new List<OfferitemPricing>
                                {
                                    new()
                                    {
                                        AddCollectPrice = null,
                                        RefundPrice = null
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var validationResponse = OrderChangeValidations.ValidatePaymentFunctions<IATA_OrderViewRS>(request,_hostEnvironment,offerDoc);

            // Assert
            Assert.NotNull(validationResponse.ResponseMessages);
            Assert.Equal("912", validationResponse.ResponseMessages.Errors.First().Code);
        }

        [Fact]
        public void ValidateDisruptionPaymentFunctions_WhenPaymentFunctionsArePresent_Returns914_19()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");

            var validationResponse = OrderChangeValidations.ValidateDisruptionPaymentFunctions<IATA_OrderViewRS>(request, _hostEnvironment);

            Assert.NotNull(validationResponse.ResponseMessages);
            Assert.Equal("914", validationResponse.ResponseMessages.Errors.First().Code);
            Assert.Equal("914_19", validationResponse.ResponseMessages.Errors.First().Id);
        }
    }
}
