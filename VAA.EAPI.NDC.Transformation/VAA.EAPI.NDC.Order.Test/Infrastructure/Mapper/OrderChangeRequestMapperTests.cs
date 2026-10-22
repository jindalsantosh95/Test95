using MediatR;
using Moq;
using System.Linq;
using System.Text.Json;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Mapper
{
    public class OrderChangeRequestMapperTests
    {
        [Fact]
        public void Mapper_ShouldReturnValid_Response()
        {
            // Arrange
            var orderChangeRq = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            var orderDoc = TestDataProvider.GetTestData<OrderRS>("NDCOrder\\OrderChange\\OrderDoc");
            var orderChangeRequest = new OrderChangeEvenExchangeCommand() { OrderChangeRQ = orderChangeRq };

            // Act
            var result = OrderChangeRequestMapper.MapOrderChangeRequest(orderChangeRequest.OrderChangeRQ, orderDoc, offerDoc);

            bool hasExpectedCustomerRef = result.OrderChangeRequest.OrderAmendments
                .SelectMany(oa => oa.CustomerReference)
                .Contains("ADULT_1");

            bool hasExpectedRefId = result.OrderChangeRequest.OrderAmendments
                .SelectMany(oa => oa.OrderByReference)
                .Any(obr => obr.ItemsForAddition.OfferRefID == "264bc469-80c8-4e7d-9045-3e54e0efb95f|V9KadFUuPrONC8zzZPMTYC003");

            bool hasService = result.OrderChangeRequest.SelectedOffers
                .Any(offer => offer.OfferItems.Any(item => item.Services != null && item.Services.Any()));

            bool hasSegmentId = orderDoc.OrderDocument.DataList.SegmentDetails
                .Any(segment => segment.SegmentId == "FSEG1");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.OrderChangeRequest.OrderAmendments);
            Assert.NotEmpty(result.OrderChangeRequest.SelectedOffers);
            Assert.NotEmpty(orderDoc.OrderDocument.DataList.CustomerDetails);
            Assert.NotNull(offerDoc.DataList.ProductComponents);
            Assert.True(hasExpectedCustomerRef);
            Assert.True(hasExpectedRefId);
            Assert.True(hasService);
            Assert.True(hasSegmentId);
            Assert.Equal("VS932DE6DIF24", orderDoc.OrderDocument.OrderNumber.OrderId);
        }

        [Fact]
        public void Mapper_ShouldRetainExistingTransactionAndUseDisruptionAmendment_WhenReshopIsDisrupted()
        {
            // Arrange
            var orderChangeRq = TestDataProvider.GetXmlTestData<IATA_OrderChangeRQ>("NDCOrder\\OrderChange\\OrderChangeReq");
            var offerDoc = TestDataProvider.GetTestData<OfferResponse>("NDCOrder\\OrderChange\\OfferDoc");
            var orderDoc = TestDataProvider.GetTestData<OrderRS>("NDCOrder\\OrderChange\\OrderDoc");
            var existingTransactionId = orderDoc.OrderDocument.OrderItems .SelectMany(orderItem => orderItem.PaymentDetails) .First().PaymentReferenceId;

            // Act
            var result = OrderChangeRequestMapper.MapOrderChangeRequest( orderChangeRq, orderDoc, offerDoc, isReshopDisruption: true);

            // Assert
            Assert.True(result.OrderChangeRequest.IsReshopDisruption);
Assert.Null(result.OrderChangeRequest.ChangeType);
            Assert.Contains(existingTransactionId, result.OrderChangeRequest.TransactionId);
            Assert.Single(result.OrderChangeRequest.OrderAmendments);
            Assert.NotNull(result.OrderChangeRequest.OrderAmendments.First().FlightItems);
        }
    }
}
