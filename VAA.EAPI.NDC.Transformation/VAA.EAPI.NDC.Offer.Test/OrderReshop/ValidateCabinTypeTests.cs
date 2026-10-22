using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.OrderReshopRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.UpsellOffers;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.OrderReshop;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop
{
    public class ValidateCabinTypeTests
    {
        [Fact]
        public void ValidateAgentInRQ_ErrorWithNullCabinType_Request()
        {
            ReshopCabinType reshopCabinType = new ReshopCabinType() { CabinCode= "334" };
            var orderReshopRequest= new OrderReshopRequest() { ReshopQueries = new() { FlightSpecific = new() { SearchCriteria = new SearchCriteria() { CabinTypes = new List<ReshopCabinType> { reshopCabinType } } } } };
            var validationErrors = ValidateOrderReshop.ValidateOrderReshopRQCabinPreferance(orderReshopRequest).ToList();
           
            Assert.Equal("914_7", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_ErrorWithEmptyOrderID_Request()
        {
            
            var orderReshopRequest = new OrderReshopRequest() { OrderId= null};
            var orderRequest = orderReshopRequest.OrderId;
            var validationErrors = ValidateOrderReshop.ValidateOrderReshopRQOrderID(orderRequest);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("914_4", validationErrors.First().ErrorCode);

        }

        [Fact]
        public void ValidateAgentInRQ_ErrorWithEmptyOrderItemID_Request()
        {
            ReshopOrder reshopOrder= new ReshopOrder() { OrderItemId=null};
            var orderReshopRequest = new OrderReshopRequest() { ReshopOrder=new List<ReshopOrder>() { reshopOrder } };
            var validationErrors = ValidateOrderReshop.ValidateOrderReshopRQOrderItemID(orderReshopRequest);

            Assert.True(validationErrors.First().ErrorCode.Any());
            Assert.Equal("914_8", validationErrors.First().ErrorCode);
        }
    }
}
