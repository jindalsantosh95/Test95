using Microsoft.Extensions.Hosting;
using Moq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.OrderQuote;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderQuote
{
    public class OrderQuoteValidationTests
    {
        private readonly Mock<IHostEnvironment> _hostEnvironment;
        public OrderQuoteValidationTests() 
        {
            _hostEnvironment = new Mock<IHostEnvironment>();
        }

        [Fact]
        public void Validate_OrderRepriceRequest()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");

            var validationErrors = MandatoryFieldsValidations.ValidateRequestFields(Request);

            Assert.False(validationErrors.Any());
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithNullOrderID()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.OrderId = null;

            var validationErrors = MandatoryFieldsValidations.ValidateRequestFields(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_5", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithNullServiceBenificiries()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.PricedOffer.SelectedOffer.OfferItems.First().ServiceBeneficiaries = null;

            var validationErrors = MandatoryFieldsValidations.ValidateRequestFields(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_5", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInValidOrdinal()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.Parties.Participants.First().Ordinal = "A";

            var validationErrors = MandatoryFieldsValidations.ValidateOrdinalinRequestFields(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_With0Ordinal()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.Parties.Participants.First().Ordinal = "0";

            var validationErrors = MandatoryFieldsValidations.ValidateOrdinalinRequestFields(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_24", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_With1Ordinal()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.Parties.Participants.First().Ordinal = "1";

            var validationErrors = MandatoryFieldsValidations.ValidateOrdinalinRequestFields(Request);

            Assert.False(validationErrors.Any());
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInvalidaOwnerCode()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.OwnerCode = "ABC";

            var validationErrors = MandatoryFieldsValidations.ValidateOrderIDinRQ(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_2", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithvalidaOwnerCode()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.OwnerCode = "VS";

            var validationErrors = MandatoryFieldsValidations.ValidateOrderIDinRQ(Request);

            Assert.True(validationErrors.Count() == 0);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInvalidaOrderID()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.OrderId = "ABC&&46E";

            var validationErrors = MandatoryFieldsValidations.ValidateOrderIDinRQ(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("129_3", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithNullOfferID()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.PricedOffer.SelectedOffer.OwnerCode = null;

            var validationErrors = MandatoryFieldsValidations.ValidateOfferIDinRQ(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_2", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInvalidOfferID()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            Request.PricedOffer.SelectedOffer.OfferRefID = "8263n@3y";

            var validationErrors = MandatoryFieldsValidations.ValidateOfferIDinRQ(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInvalidOffers()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");

            if (Request.PricedOffer.SelectedOffer.OfferItems != null)
            {
                Request.PricedOffer.SelectedOffer.OfferItems.First().OfferItemId = "sjjdfuj%wrkj";
            }

            var validationErrors = MandatoryFieldsValidations.ValidateOfferIDinRQ(Request);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInvalidOrderInDB()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            var offerDocRequest = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponseValidations");
            offerDocRequest.OrderId = "VS932DGHLFK24";

            var validationErrors = MandatoryFieldsValidations.ValidateOrderDB(Request, offerDocRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("129_3", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInvalidOfferItems()
        {
            var Request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            var offerDocRequest = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponseValidations");

            var validationErrors = MandatoryFieldsValidations.ValidateOrderDB(Request, offerDocRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_1", validationErrors[0].ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithNullOrderDoc()
        {
            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderQuote\\OrderDocument");
            orderDocRequest = null;

            var validationErrors = MandatoryFieldsValidations.ValidateOrderDetails(orderDocRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("129_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInvalidWarnings()
        {
            var orderDocRequest = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\OrderQuote\\OrderDocument");
            orderDocRequest.Warnings.First().Code = "139";

            var validationErrors = MandatoryFieldsValidations.ValidateOrderDetails(orderDocRequest);

            Assert.True(validationErrors.Count() == 0);
        }

        [Fact]
        public void Validate_OrderRepriceRequest_WithInvalidPaxList()
        {
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            var offerDocRequest = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponseValidations");
            offerDocRequest.DataList.CustomerDetails.First().CustId = null;

            var validationErrors = MandatoryFieldsValidations.ValidatePaxList(request, offerDocRequest);

            Assert.True(validationErrors.Any());
            Assert.Equal("912_4", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_OrderQuote_Response()
        {
            var offerRS = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            offerRS.Error = new List<ErrorDetails>();
            _hostEnvironment.Setup(h => h.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            var validationErrors = OrderQuoteResponseValidations.ValidateOrderQuoteResponse(offerRS, true, _hostEnvironment.Object);

            Assert.NotNull(validationErrors);
            Assert.Equal("BusinessException", validationErrors.Status.ToString());
            Assert.Equal("Validation error", validationErrors.ErrorMessage);
            Assert.Equal("913", validationErrors.ResponseMessages.Errors.First().Code);
        }

        [Fact]
        public void Validate_OfferItem_InDB_NullOfferDoc()
        {
           var request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
           var offerRS = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\ValidOfferdoc");
           var validationErrors = MandatoryFieldsValidations.ValidateOfferItemIDinDB(request, null);

           Assert.True(validationErrors.Any());
           Assert.Equal("913_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_OfferItem_InDB_NullOffers()
        {
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            var offerRS = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\ValidOfferdoc");
            offerRS.Offers = null;
            var validationErrors = MandatoryFieldsValidations.ValidateOfferItemIDinDB(request, offerRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_OfferItem_InDB_WithVlaidRequest()
        {
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            var offerRS = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            offerRS.Offers.First().OfferId = "ae702729-989a-473f-b2a3-c0017e023aa5|OTa6Ys5n0r9V4iKHdZYO3C00B";
            var validationErrors = MandatoryFieldsValidations.ValidateOfferItemIDinDB(request, offerRS);

            Assert.True(validationErrors.Any());
            Assert.Equal("913_1", validationErrors.First().ErrorCode);
        }

        [Fact]
        public void Validate_OrderDB_Service_Beneficiaries()
        {
            var request = TestDataProvider.GetTestData<OrderRepriceQuery>("NDCOffer\\OrderQuote\\OrderRepriceValidRequest");
            var offerRS = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\OrderQuote\\OfferResponse");
            var validationErrors = MandatoryFieldsValidations.ValidateOrderDBServiceBeneficiaries(request, offerRS);

            Assert.False(validationErrors.Any());
        }

    }
}
