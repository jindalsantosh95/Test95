using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Validations.SeatAvailability;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.SeatAvailability
{
    public class ValidateSeatAvailabilityOfferDetailsTest
    {
        [Fact]
        public void Should_throw_error_when_seatOffer_is_null_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            (request.Request?.SeatAvailCoreRequest?.Item as SeatAvailOfferRequestType).Offer.First().OfferID = null;

            var responseMessage = ValidateSeatAvailability.ValidateSeatAvailabilityOfferID(request);

            Assert.True(responseMessage.Any());
            Assert.Equal("911_5", responseMessage.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_seatOffer_is_Empty_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            (request.Request.SeatAvailCoreRequest.Item as SeatAvailOfferRequestType).Offer.First().OfferID = string.Empty;
            var responseMessage = ValidateSeatAvailability.ValidateSeatAvailabilityOfferID(request);

            Assert.True(responseMessage.Any());
            Assert.Equal("911_5", responseMessage.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OwnerCode_is_null_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            (request.Request.SeatAvailCoreRequest.Item as SeatAvailOfferRequestType).Offer.First().OwnerCode = null;

            var responseMessage = ValidateSeatAvailability.ValidateSeatAvailabilityOfferID(request);

            Assert.True(responseMessage.Any());
            Assert.Equal("911_10", responseMessage.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OwnerCode_is_Empty_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            (request.Request.SeatAvailCoreRequest.Item as SeatAvailOfferRequestType).Offer.First().OwnerCode = string.Empty;

            var responseMessage = ValidateSeatAvailability.ValidateSeatAvailabilityOfferID(request);

            Assert.True(responseMessage.Any());
            Assert.Equal("911_10", responseMessage.Last().ErrorCode);
        }

        [Fact]
        public void Should_not_throw_error_when_All_Mandatory_field_is_available_in_seatAvailability_request()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            var responseMessage = ValidateSeatAvailability.ValidateSeatAvailabilityOfferID(request);

            Assert.False(responseMessage.Any());
        }

        [Fact]
        public void Should_throw_error_when_OfferDoc_is_null()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            offerDoc.Offers.First().OfferId = null;

            var offerId = "f3a580a7-4dbf-4084-b662-a62d48c6ebe9|QlOmQmLRmVBUJlYFU82W43001";
            var responseMessage = ValidateSeatAvailability.ValidateSeatOfferDetailsInOfferDoc(request, offerDoc, offerId);

            Assert.True(responseMessage.Any());
            Assert.Equal("911_9", responseMessage.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OfferDoc_contains_Error()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            offerDoc.Error = new List<ErrorDetails> { new() { Message = "Failed" } };

            var offerId = "f3a580a7-4dbf-4084-b662-a62d48c6ebe9|QlOmQmLRmVBUJlYFU82W43001";
            var responseMessage = ValidateSeatAvailability.ValidateSeatOfferDetailsInOfferDoc(request, offerDoc, offerId);

            Assert.True(responseMessage.Any());
            Assert.Equal("911_9", responseMessage.First().ErrorCode);
        }

        [Fact]
        public void Should_throw_error_when_OfferDoc_OfferID_does_not_match_request_offerID()
        {
            var request = TestDataProvider.GetXmlTestData<IATA_SeatAvailabilityRQ>("NDCOffer\\SeatAvailability\\PrimeFlowSeatAvailabilityRQ");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOffer\\SeatAvailability\\OfferDoc");
            offerDoc.Error = new List<ErrorDetails> { new() { Message = "Failed" } };
            var offerId = "f3a580a7-4dbf-4084-b662-a62d48c6ebe";
            var responseMessage = ValidateSeatAvailability.ValidateSeatOfferDetailsInOfferDoc(request, offerDoc, offerId);

            Assert.True(responseMessage.Any());
            Assert.Equal("911_9", responseMessage.First().ErrorCode);
        }
    }
}
