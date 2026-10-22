using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.FlightServiceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderCreate
{
    public class OrderTicketRQMapperTest
    {

        [Fact]
        public void GetAddress_ShouldReturnCorrectAddress_WhenValidInput()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\PositiveFlow\\OfferDoc");
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            var flightServiceListResponse = TestDataProvider.GetTestData<FlightOfferServiceRS>("NDCOrder\\OrderCreate\\PositiveFlow\\ServiceListOfferResponse");

            var result = OrderTicketMSRequestMapper.MapOrderTicketMSRequest(request, offerDoc, orderDoc, "80617469");
            var address = result.CustomerDetails.First().Addresses;

            Assert.Equal("D", address.AddressType);
            Assert.Equal("Room 101", address.AddressLine1);
            Assert.Equal("Main St", address.AddressLine2);
            Assert.Equal("600001", address.AddressLine3);
            Assert.Equal("Chennai", address.City);
            Assert.Equal("Tamil Nadu", address.Region);
            Assert.Equal("India", address.Country);
            Assert.Equal("600001", address.PostalCode);
        }

        [Fact]
        public void GetTravelDocuments_ShouldReturnCorrectDocuments_WhenRedressCaseIsNotNull()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\PositiveFlow\\OfferDoc");
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            var flightServiceListResponse = TestDataProvider.GetTestData<FlightOfferServiceRS>("NDCOrder\\OrderCreate\\PositiveFlow\\ServiceListOfferResponse");

            var result = OrderTicketMSRequestMapper.MapOrderTicketMSRequest(request, offerDoc, orderDoc, "80617469");
            var document = result.CustomerDetails.First().TravelDocuments.First();

            Assert.Equal(Constants.Application.AEA, document.DocumentType);
            Assert.Equal("1234567", document.DocumentNumber);
            Assert.Equal("USA", document.CountryOfIssue);
            Assert.Equal("DAVID", document.PersonName.FirstName);
            Assert.Equal("SMITH", document.PersonName.LastName);
        }

        [Fact]
        public void GetTravelDocuments_ShouldReturnCorrectDocuments_WhenIdentityDocIsNotNull()
        {
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderCreate\\PositiveFlow\\OfferDoc");
            var request = TestDataProvider.GetXmlTestData<IATA_OrderCreateRQ>("NDCOrder\\OrderCreate\\OrderCreateRQ");
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDoc");
            var flightServiceListResponse = TestDataProvider.GetTestData<FlightOfferServiceRS>("NDCOrder\\OrderCreate\\PositiveFlow\\ServiceListOfferResponse");

            var result = OrderTicketMSRequestMapper.MapOrderTicketMSRequest(request, offerDoc, orderDoc, "80617469");
            var document = result.CustomerDetails.First().TravelDocuments.First(x => x.DocumentType == "GC");

            Assert.Equal(DateTime.Parse("1946-08-05").Ticks, DateTime.Parse(document.DateOfBirth).Ticks);
            Assert.Equal(DateTime.Parse("2026-06-03").Ticks, DateTime.Parse(document.ExpirationDate).Ticks);
            Assert.Equal(DateTime.Parse("2016-06-03").Ticks, DateTime.Parse(document.IssueDate).Ticks);
            Assert.Equal("GBR", document.CountryOfResidence);
            Assert.Equal("Danial", document.PersonName.FirstName);
            Assert.Equal("Smith", document.PersonName.LastName);
            Assert.Equal("USA0001235", document.DocumentNumber);
            Assert.Equal("GC", document.DocumentType);
            Assert.Equal("USA", document.CountryOfIssue);
            Assert.Equal("GBR", document.Nationality);
        }
    }
}
