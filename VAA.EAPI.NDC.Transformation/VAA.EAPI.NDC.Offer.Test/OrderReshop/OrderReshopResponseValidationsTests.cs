using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop
{
    public class OrderReshopResponseValidationsTests
    {
        private readonly IHostEnvironment _hostEnvironment;

        public OrderReshopResponseValidationsTests()
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
        }

        [Fact]
        public void ReshopValidationWhenNoFlightFound()
        {
            var offerDocument = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderReshop\\ReshopOfferResponse");
            var validationErrors = OrderReshopResponseValidations.ValidateReshopOfferMS<ErrorType>(offerDocument, _hostEnvironment);

            Assert.NotNull(validationErrors);
            Assert.Equal("100_2", validationErrors.ResponseMessages.Errors.First().Id);
        }
    }


    }

