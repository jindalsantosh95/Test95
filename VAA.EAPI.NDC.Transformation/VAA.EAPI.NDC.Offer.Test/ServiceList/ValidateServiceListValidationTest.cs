using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.ServiceListRQ;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Offer.Validations.ServiceListValidations;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using Warning = VAA.EAPI.Domain.Entities.Common.Warning;

namespace VAA.EAPI.NDC.Offer.Tests.ServiceList
{
    public class ValidateServiceListValidationTest
    {

        [Fact]
        public void Validate_Empty_ServiceOrder_In_SericeRequest()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOrderRQ");
            ((ServiceListOrderRequestType)request.Request.ServiceListCoreRequest.Item).Order.OrderID = string.Empty;

            //Act
            var validationErrors = ValidateServiceList.ValidateServiceOrderDetails(request);

            //Assert
            Assert.True(validationErrors.First().ErrorCode == "912_1");
        }

        [Fact]
        public void Validate_Null_OwnerCode_In_SericeRequest()
        {
            //Arrange
            var request = TestDataProvider.GetXmlTestData<IATA_ServiceListRQ>("NDCOffer\\ServiceList\\ServiceListOrderRQ");
            ((ServiceListOrderRequestType)request.Request.ServiceListCoreRequest.Item).Order.OwnerCode = string.Empty;

            //Act
            var validationErrors = ValidateServiceList.ValidateServiceOrderDetails(request);

            //Assert
            Assert.True(validationErrors.First().ErrorCode == "912_1");
        }

        [Fact]
        public void Validate_Positive_Flow_OrderDocument()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDoc");

            //Act
            var validationErrors = ValidateServiceList.ValidateOrderDetails(orderDoc);

            //Assert
            Assert.True(validationErrors.Count() == 0);
        }

        [Fact]
        public void Validate_ErrorCode_129_In_OrderDocument()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDoc");
            orderDoc.Warnings = new List<Warning> { new() { Code = "129" } };

            //Act
            var validationErrors = ValidateServiceList.ValidateOrderDetails(orderDoc);

            //Assert
            Assert.True(validationErrors.First().ErrorCode == "76H_1");
        }

        [Fact]
        public void Validate_ErrorCode_914_In_OrderDocument()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDoc");
            orderDoc.Warnings = new List<Warning> { new() { Code = "914" } };

            //Act
            var validationErrors = ValidateServiceList.ValidateOrderDetails(orderDoc);

            //Assert
            Assert.True(validationErrors.First().ErrorCode == "76H_1");
        }

        [Fact]
        public void Validate_Unknown_OrderWarning_In_OrderDocument()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDoc");
            orderDoc.Warnings = new List<Warning> { new() { Code = "000" } };

            //Act
            var validationErrors = ValidateServiceList.ValidateOrderDetails(orderDoc);

            //Assert
            Assert.True(validationErrors.Count() == 0);
        }

        [Fact]
        public void Validate_FLOWN_USED_CouponStatus_In_OrderDocument()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDoc");
            orderDoc.OrderItems.First().AccountableDocumentRefs.First().Coupons.First().CouponStatus = "FLOWN/USED";

            //Act
            var validationErrors = ValidateServiceList.ValidateOrderDetails(orderDoc);

            //Assert
            Assert.True(validationErrors.First().ErrorCode == "76H_1");
        }

        [Fact]
        public void Validate_Non_VS_Carrier_Segment_In_OrderDocument()
        {
            //Arrange
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOffer\\ServiceList\\OrderDoc");
            foreach (var seg in orderDoc.DataList.SegmentDetails)
            {
                if (seg.OperatingCarrier == null)
                {
                    seg.OperatingCarrier = "DL";
                }
            };

            //Act
            var validationErrors = ValidateServiceList.ValidateOrderDetails(orderDoc);

            //Assert
            Assert.True(validationErrors.First().ErrorCode == "76H_1");
        }
    }
}

