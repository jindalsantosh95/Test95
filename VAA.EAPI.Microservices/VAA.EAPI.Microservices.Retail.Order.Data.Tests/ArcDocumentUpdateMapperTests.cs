using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class ArcDocumentUpdateMapperTests
    {
        [Fact]
        public void ArcDocumentUpdaterMapperTests()
        {
            var orderDoc = TestDataProvider.GetTestData<List<OrderDocument>>("SabreOrderChangeMgt\\OrderDocument");
            var arcDoc = TestDataProvider.GetTestData<ARCDocument>("SabreOrderChangeMgt\\ARCDocument");

            var result = OrderArcDocumentUpdateMapper.Update(orderDoc.First(), arcDoc);

            Assert.NotNull(result);
            Assert.Equal("VS932RTYRKP26-ARC", result.Id);
            Assert.Equal("ADULT_1", result.ARCTransactionHistory.First().CustomerId);
            Assert.Equal("9322520229785", result.ARCTransactionHistory.First().PassengerTicketDetails.First().AccountableDocumentRefs.First().DocumentNumber);
            Assert.Equal("OrderCreate", result.ARCTransactionHistory.First().PassengerTicketDetails.First().Type);
        }
    }
}
