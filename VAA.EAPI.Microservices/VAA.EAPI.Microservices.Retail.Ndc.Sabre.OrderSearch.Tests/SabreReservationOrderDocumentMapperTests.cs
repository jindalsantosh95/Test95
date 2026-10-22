using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using PnrRemark = VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy.Remark;
using ApplicationConstants = VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Constants.Application;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Tests
{
    public class SabreReservationOrderDocumentMapperTests
    {
        [Fact(Skip ="Response Mapping is in progress")]
        public void ReservationRS_FlightSegment_ShouldMap_OrderDocument_FlightSegment()
        {
            //Arrange 

                var currentOrderDocument = TestDataProvider.GetTestData<OrderDocument>("SabreResponses\\NDCOrderDocument\\OrderDocument_ResponseSegmentInfo");
                var reservationRS = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservation_ResponseSegmentInfo");

                //Act
                var mappedOrderDocument = SabreResponseToOrderDocumentMapper.Map(new(), reservationRS,null,null);
                var segmentOrigin = ((ReservationPNRB)reservationRS.Item).PassengerReservation.Segments.Poc.Airport;
                var orderSegment = mappedOrderDocument.DataList.SegmentDetails.First(x => x.OriginAirportCode == segmentOrigin);
                var airSegment = (AirType)((ReservationPNRB)reservationRS.Item).PassengerReservation.Segments.Segment[0].Item;

                //Assert
                Assert.NotNull(mappedOrderDocument);
                Assert.Equal(orderSegment.AirCraft, airSegment.EquipmentType);
                Assert.Equal(orderSegment.ClassOfService, airSegment?.ClassOfService);
                Assert.Equal(orderSegment.CabinType.CabinCode, airSegment?.Cabin.Code);
                Assert.Equal(orderSegment.NumberInParty, airSegment?.NumberInParty.ToString());
                Assert.Equal(orderSegment.MarketingFlightNumber, airSegment?.MarketingFlightNumber);
                Assert.Equal(orderSegment.OriginCityCode, airSegment?.DepartureAirportCodeContext);
                Assert.Equal(orderSegment.DestinationAirportCode, airSegment?.ArrivalAirport);
           
        }

        [Fact]
        public void MapperTestForWaitlistedSSRs()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchRequest>("OrderSearchMS\\OrderSearchRQ_ForWaitlistedSSR");
            var getReservRes = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationRS_WithWaitlistedSSR");

            //Act
            var result = SabreResponseToOrderDocumentMapper.Map(request , getReservRes , null , null);

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result.DataList.OrderInstructions);
            Assert.True(result.Warnings.Any());
            Assert.Contains(result.Warnings, x => x.Code == "182");
        }

        [Fact]
        public void Mapper_ShouldMapPaxAssociatedCtceAndCtcmContacts()
        {
            var request = TestDataProvider.GetTestData<OrderSearchRequest>("OrderSearchMS\\OrderSearchRQ_ForWaitlistedSSR");
            var getReservRes = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationRS_WithWaitlistedSSR");
            var reservation = (ReservationPNRB)getReservRes.Item;
            var passenger = reservation.PassengerReservation.Passengers.Passenger.Single();
            var originalCustomer = request.SearchFilterCriteria.CustomerDetails.Single();
            originalCustomer.Contacts = new()
            {
                CountryDailingCode = "44",
                Phone = "7000000000"
            };
            var emailRequest = reservation.OpenReservationElements.OpenReservationElement
                .Select(element => element.Item)
                .OfType<ServiceRequestType>()
                .Single(serviceRequest => serviceRequest.code == "CTCE");
            ((ServiceRequestTypePassengerContactEmail)emailRequest.Item).Email = "traveller@example.test";
            reservation.OpenReservationElements.OpenReservationElement = [
                .. reservation.OpenReservationElements.OpenReservationElement,
                new OpenReservationElementType
                {
                    Item = new ServiceRequestType
                    {
                        code = "CTCM",
                        Item = new ServiceRequestTypePassengerContactMobilePhone { PhoneNumber = "447000000000" }
                    },
                    NameAssociation = [new NameAssociationTag
                    {
                        FirstName = passenger.FirstName,
                        LastName = passenger.LastName,
                        NameRefNumber = passenger.nameId
                    }]
                },
                new OpenReservationElementType
                {
                    Item = new ServiceRequestType { code = "CTCR" },
                    NameAssociation = [new NameAssociationTag { NameRefNumber = passenger.nameId }]
                }
            ];

            var result = SabreResponseToOrderDocumentMapper.Map(request, getReservRes, null, null);

            var customer = result.DataList.CustomerDetails.Single();
            Assert.Equal("traveller@example.test", customer.Contacts.Email);
            Assert.Equal("7000000000", customer.Contacts.Phone);
            Assert.Equal("44", customer.Contacts.CountryDailingCode);
            Assert.Null(customer.Contacts.PhoneType);
            Assert.False(customer.Contacts.ContactRefusedInd);
        }

        [Fact]
        public void Mapper_ShouldMapPaxAssociatedCtcrWithoutContactDetails()
        {
            var request = TestDataProvider.GetTestData<OrderSearchRequest>("OrderSearchMS\\OrderSearchRQ_ForWaitlistedSSR");
            var getReservRes = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationRS_WithWaitlistedSSR");
            var reservation = (ReservationPNRB)getReservRes.Item;
            var passenger = reservation.PassengerReservation.Passengers.Passenger.Single();
            reservation.OpenReservationElements.OpenReservationElement = [
                .. reservation.OpenReservationElements.OpenReservationElement
                    .Where(element => (element.Item as ServiceRequestType)?.code != "CTCE"),
                new OpenReservationElementType
                {
                    Item = new ServiceRequestType { code = "CTCR" },
                    NameAssociation = [new NameAssociationTag { NameRefNumber = passenger.nameId }]
                }
            ];

            var result = SabreResponseToOrderDocumentMapper.Map(request, getReservRes, null, null);

            var customer = result.DataList.CustomerDetails.Single();
            Assert.True(customer.Contacts.ContactRefusedInd);
            Assert.Null(customer.Contacts.Email);
            Assert.Null(customer.Contacts.Phone);
        }

        [Fact]
        public void GetProductScheduleChangeIndicator_ShouldReturnValueFromProductAir()
        {
            // Arrange
            var product = new ProductType
            {
                ProductDetails = new ProductDetailsType
                {
                    Item = new AirType2 { ScheduleChangeIndicator = true }
                }
            };
            var getProductScheduleChangeIndicator = typeof(SabreResponseToOrderDocumentMapper).GetMethod(
                "GetProductScheduleChangeIndicator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var scheduleChangeIndicator = (bool)getProductScheduleChangeIndicator!.Invoke(null, new object[] { product })!;

            // Assert
            Assert.True(scheduleChangeIndicator);
        }

        [Fact]
        public void MapperTestForOCN_HoldBookingIssuanceTrigger_ShouldMapDocumentIssuanceAndConfirmedOrderStatus_WhenTicketStatusIsUnknown()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<OrderSearchRequest>("OrderSearchMS\\OrderSearchRQ_ForOCN_DocumentIssuance");
            var getReservRes = TestDataProvider.GetXmlTestData<GetReservationRS>("SabreResponses\\GetReservationRS_ForOCN_DocumentIssuance");

            //Act
            var result = SabreResponseToOrderDocumentMapper.MapForOCN(request, getReservRes, null, null);

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result.OrderHistory);
            Assert.Equal(ChangeTypeEnum.DocumentIssuance, result.OrderHistory.First().ChangeType);
            Assert.Equal(ApplicationConstants.OrderStatusConfirmed, result.OrderStatus);
        }

        [Fact]
        public void IsTicketExchanged_ShouldReturnTrue_WhenMultipleTicketIssueFactsExistBeforeExchangeFact()
        {
            // Arrange
            var reservation = new ReservationPNRB
            {
                History = new[]
                {
                    CreateTicketFactHistoryTransaction(sequenceNbr: 3, historyAction: "A3S"),
                    CreateTicketFactHistoryTransaction(sequenceNbr: 2, historyAction: "A3S"),
                    CreateTicketFactHistoryTransaction(sequenceNbr: 1, historyAction: "X3S*")
                }
            };

            // Act
            var result = InvokeIsTicketExchanged(reservation);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTicketExchanged_ShouldReturnFalse_WhenNoExchangeFactFollowsTicketIssueFacts()
        {
            // Arrange
            var reservation = new ReservationPNRB
            {
                History = new[]
                {
                    CreateTicketFactHistoryTransaction(sequenceNbr: 3, historyAction: "A3S"),
                    CreateTicketFactHistoryTransaction(sequenceNbr: 2, historyAction: "A3S"),
                    CreateTicketFactHistoryTransaction(sequenceNbr: 1, historyAction: "A4S")
                }
            };

            // Act
            var result = InvokeIsTicketExchanged(reservation);

            // Assert
            Assert.False(result);
        }

       
        private static bool InvokeIsTicketExchanged(ReservationPNRB reservation)
        {
            var method = typeof(SabreResponseToOrderDocumentMapper).GetMethod(
                "IsTicketExchanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.NotNull(method);

            return (bool)method.Invoke(null, new object[] { reservation })!;
        }

        private static ReservationPNRBHistoryTransaction CreateTicketFactHistoryTransaction(int sequenceNbr, string historyAction)
        {
            return new ReservationPNRBHistoryTransaction
            {
                Signature = new SignatureType1 { SequenceNbr = sequenceNbr, SequenceNbrSpecified = true },
                Items = new object[]
                {
                    new FactHistoryType
                    {
                        Code = "TKNE",
                        HistoryAction = historyAction
                    }
                }
            };
        }
    }
}
