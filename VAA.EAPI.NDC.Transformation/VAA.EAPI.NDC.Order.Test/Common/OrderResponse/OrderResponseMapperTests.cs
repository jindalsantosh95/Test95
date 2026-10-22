using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using VAA.EAPI.NDC.Order.Validations.OrderRetrieve;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.Common.OrderResponse
{
    public class OrderResponseMapperTests
    {
        private readonly IHostEnvironment _hostEnvironment;

        public OrderResponseMapperTests()
        {
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
        }

        [Fact]
        public void HoldBooking_ResponseMapper_DoesNotReturnPaymentFunctions()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");

            var response = OrderResponseMapper.MapOrderResponse(orderDoc, null, true);

            Assert.Null(response.PaymentFunctions);
        }

        [Fact]
        public void PaxDetails_ResponseMapper_For5PAX_Return_Tests()
        {

            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Birthdate == Convert.ToDateTime(doc.DateOfBirth));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.GivenName.First() == doc.FirstName);
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Surname == doc.LastName);
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Surname == doc.LastName);
            }
        }

        [Fact]
        public void PaxSegment_ResponseMapper_For5PAX_Return_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.SegmentDetails)
            {
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => (x.CabinTypeAssociationChoice.Item as CabinTypeType)?.CabinTypeCode == doc.CabinType.CabinCode);
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => (x.CabinTypeAssociationChoice.Item as CabinTypeType)?.CabinTypeName == doc.CabinType.CabinName);
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => x.MarketingCarrierRBD_Code == doc.ClassOfService);
            }
        }

        [Fact]
        public void OriginDestinationandJourney_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.OriginDestinationAssociations)
            {
                Assert.Contains(response.DataLists.OriginDestList.Where(x => x.OriginDestID == doc.OdLabel), x => x.DestCode == doc.OriginDestination.DestinationAirport);
                Assert.Contains(response.DataLists.OriginDestList.Where(x => x.OriginDestID == doc.OdLabel), x => x.OriginCode == doc.OriginDestination.OriginAirport);
                Assert.Equal(orderDoc.DataList.Journey.First().JourneyId, response.DataLists.PaxJourneyList.First().PaxJourneyID);
            }
        }

        [Fact]
        public void OriginDestination_ResponseMapper_UsesJourneySegmentEndpoints_WhenStoredOriginDestinationIsInconsistent()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var originDestinationAssociation = orderDoc.DataList.OriginDestinationAssociations.First();
            var journey = orderDoc.DataList.Journey.First(journey => originDestinationAssociation.JourneyRefIds.Contains(journey.JourneyId));
            var firstJourneySegment = orderDoc.DataList.SegmentDetails.First(segment => segment.SegmentId == journey.FlightSegmentRefIds.First());
            var lastJourneySegment = orderDoc.DataList.SegmentDetails.First(segment => segment.SegmentId == journey.FlightSegmentRefIds.Last());

            originDestinationAssociation.OriginDestination.OriginAirport = "XXX";
            originDestinationAssociation.OriginDestination.DestinationAirport = "YYY";

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            var originDestination = response.DataLists.OriginDestList.First(originDest => originDest.OriginDestID == originDestinationAssociation.OdLabel);

            Assert.Equal(firstJourneySegment.OriginAirportCode, originDestination.OriginCode);
            Assert.Equal(lastJourneySegment.DestinationAirportCode, originDestination.DestCode);
        }

        [Fact]
        public void OperatingLegDetails_ResponseMapper_For5PAX_Return_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.SegmentDetails)
            {
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.CarrierDesigCode == doc.MarketingCarrier);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.MarketingCarrierFlightNumberText == doc.MarketingFlightNumber);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Arrival.IATA_LocationCode == doc.DestinationAirportCode);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Dep.AircraftScheduledDateTime == Convert.ToDateTime(doc.ScheduledDepartureTime.LocalDateTime));
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Dep.IATA_LocationCode == doc.OriginAirportCode);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Arrival.AircraftScheduledDateTime == Convert.ToDateTime(doc.ScheduledArrivalTime.LocalDateTime));
            }
        }

        [Fact]
        public void BaggageAllowance_ResponseMapper_For5PAX_Return_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.BaggageAllowances)
            {
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => x.TypeCode == doc.BaggageAllowanceTypeCode);
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => x.TypeCode == doc.BaggageAllowanceTypeCode);
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => doc.FreeBaggageAllowance?.AllowedBagCnt.HasValue == false || x.PieceAllowance.TotalQty == doc.FreeBaggageAllowance?.AllowedBagCnt);
            }
        }

        [Fact]
        public void ServiceDefinition_ResponseMapper_MapsServiceWithoutDescription()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var serviceDefinition = orderDoc.DataList.ServiceDefinitions.First();
            serviceDefinition.Description = null;

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.Contains(response.DataLists.ServiceDefinitionList, service => service.ServiceDefinitionID == "SRV1");
        }

        [Fact]
        public void TicketDocument_ResponseMapper_IgnoresDocumentWithoutCoupons()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            orderDoc.OrderItems.First(orderItem => orderItem.AccountableDocumentRefs?.Any() == true).AccountableDocumentRefs.First().Coupons = null;

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

Assert.DoesNotContain(response.TicketDocInfo, ticket => ticket.PaxRefID == orderDoc.OrderItems.First(orderItem => orderItem.AccountableDocumentRefs?.Any() == true).AccountableDocumentRefs.First().CustomerRefId);
        }

        [Fact]
        public void Priceclass_ResponseMapper_For5PAX_Return_Tests()
        {

            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.PriceClass)
            {
                Assert.Contains(response.DataLists.PriceClassList.Where(x => x.PriceClassID.Replace("PC", "") == doc.Id.Replace("PC", "")), x => x.FareBasisCode == doc.FareBasisCode);
                Assert.Contains(response.DataLists.PriceClassList.Where(x => x.PriceClassID.Replace("PC", "") == doc.Id.Replace("PC", "")), x => x.Name == doc.ProductName);
            }
        }

        [Fact]
        public void ContactInfo_ResponseMapper_For5PAX_Return_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Equal(doc.Contacts.Email, response.DataLists.ContactInfoList.FirstOrDefault()?.EmailAddress.FirstOrDefault()?.EmailAddressText);
                Assert.Equal(doc.Contacts.Phone, response.DataLists.ContactInfoList.FirstOrDefault()?.Phone.FirstOrDefault()?.PhoneNumber);
                Assert.Equal(doc.Contacts.CountryDailingCode, response.DataLists.ContactInfoList.FirstOrDefault()?.Phone.FirstOrDefault()?.CountryDialingCode);
            }
        }

        [Fact]
        public void OrderCreateContactInfo_ResponseMapper_ShouldMapDifferentContactsForEachEligiblePassenger()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var customers = orderDoc.DataList.CustomerDetails.ToArray();

            foreach (var customer in customers)
            {
                customer.Contacts = null;
            }

            var adult = customers.First(customer => customer.Ptc == "ADT");
            var youngPassenger = customers.First(customer => customer.Ptc == "GBE");
            var child = customers.First(customer => customer.Ptc == "CHD");
            var infant = customers.First(customer => customer.Ptc == "INF");
            adult.Contacts = new Contacts { Email = "adult@example.test", Phone = "447000000001", CountryDailingCode = "44", PhoneType = "M" };
            youngPassenger.Contacts = new Contacts { Email = "young@example.test", Phone = "447000000002", CountryDailingCode = "44", PhoneType = "M" };
            child.Contacts = new Contacts { Email = "child@example.test", Phone = "447000000003", CountryDailingCode = "44", PhoneType = "M" };
            infant.Contacts = new Contacts { Email = "infant@example.test", Phone = "447000000004", CountryDailingCode = "44", PhoneType = "M" };

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            var adultPassenger = response.DataLists.PaxList.Single(pax => pax.PaxID == adult.CustId);
            var youngResponsePassenger = response.DataLists.PaxList.Single(pax => pax.PaxID == youngPassenger.CustId);
            var childPassenger = response.DataLists.PaxList.Single(pax => pax.PaxID == child.CustId);
            var infantPassenger = response.DataLists.PaxList.Single(pax => pax.PaxID == infant.CustId);

            Assert.Equal(3, response.DataLists.ContactInfoList.Length);
            Assert.Equal("adult@example.test", response.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == adultPassenger.ContactInfoRefID).EmailAddress.Single().EmailAddressText);
            Assert.Equal("young@example.test", response.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == youngResponsePassenger.ContactInfoRefID).EmailAddress.Single().EmailAddressText);
            Assert.Equal("child@example.test", response.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == childPassenger.ContactInfoRefID).EmailAddress.Single().EmailAddressText);
            Assert.Equal("447000000001", response.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == adultPassenger.ContactInfoRefID).Phone.Single().PhoneNumber);
            Assert.Equal("447000000002", response.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == youngResponsePassenger.ContactInfoRefID).Phone.Single().PhoneNumber);
            Assert.Equal("447000000003", response.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == childPassenger.ContactInfoRefID).Phone.Single().PhoneNumber);
            Assert.Null(infantPassenger.ContactInfoRefID);
        }

        [Fact]
        public void ContactInfo_ResponseMapper_ShouldShowRefusalOnlyWhenPassengerHasNoContactDetails()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var customers = orderDoc.DataList.CustomerDetails.ToArray();
            foreach (var customer in customers)
            {
                customer.Contacts = null;
            }

            var adult = customers.First(customer => customer.Ptc == "ADT");
            var youngPassenger = customers.First(customer => customer.Ptc == "GBE");
            var child = customers.First(customer => customer.Ptc == "CHD");
            var infant = customers.First(customer => customer.Ptc == "INF");
            adult.Contacts = new Contacts { ContactRefusedInd = true };
            youngPassenger.Contacts = new Contacts { Email = "young@example.test", Phone = "7000000000", ContactRefusedInd = true };
            child.Contacts = null;
            infant.Contacts = new Contacts { ContactRefusedInd = true };

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).Single();
            var adultPassenger = response.DataLists.PaxList.Single(passenger => passenger.PaxID == adult.CustId);
            var youngResponsePassenger = response.DataLists.PaxList.Single(passenger => passenger.PaxID == youngPassenger.CustId);

            Assert.Equal(2, response.DataLists.ContactInfoList.Length);
            var adultContact = response.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == adultPassenger.ContactInfoRefID);
            Assert.True(adultContact.ContactRefusedInd);
            Assert.True(adultContact.ContactRefusedIndSpecified);
            Assert.Null(adultContact.EmailAddress);
            Assert.Null(adultContact.Phone);

            var youngContact = response.DataLists.ContactInfoList.Single(contact => contact.ContactInfoID == youngResponsePassenger.ContactInfoRefID);
            Assert.False(youngContact.ContactRefusedIndSpecified);
            Assert.Equal("young@example.test", youngContact.EmailAddress.Single().EmailAddressText);
            Assert.Equal("7000000000", youngContact.Phone.Single().PhoneNumber);
            Assert.Null(response.DataLists.PaxList.Single(passenger => passenger.PaxID == child.CustId).ContactInfoRefID);
            Assert.Null(response.DataLists.PaxList.Single(passenger => passenger.PaxID == infant.CustId).ContactInfoRefID);
        }

        [Fact]
        public void OrderCreateContactInfo_ResponseMapper_ShouldMapSeparateContactsForIdenticalDetails()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var customers = orderDoc.DataList.CustomerDetails.ToArray();
            var contact = new Contacts { Email = "shared.contact@example.test", Phone = "447000000001", CountryDailingCode = "44", PhoneType = "M" };

            foreach (var customer in customers)
            {
                customer.Contacts = customer.Ptc == "INF" ? null : contact;
            }

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            var eligiblePassengers = response.DataLists.PaxList.Where(passenger => passenger.PTC != "INF").ToArray();

            Assert.Equal(eligiblePassengers.Length, response.DataLists.ContactInfoList.Length);
            Assert.Equal(eligiblePassengers.Length, eligiblePassengers.Select(passenger => passenger.ContactInfoRefID).Distinct().Count());
            Assert.All(response.DataLists.ContactInfoList, responseContact =>
            {
                Assert.Equal("shared.contact@example.test", responseContact.EmailAddress.Single().EmailAddressText);
                Assert.Equal("447000000001", responseContact.Phone.Single().PhoneNumber);
            });
        }

        [Fact(Skip = "To be updated post payment function implementation")]
        public void PriceComponentTypeIsNull_ResponseMapper_For5PAX_Return_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            orderDoc.OrderItems.First().PriceDetails.First().PriceComponentType = null;

            var result = OrderResponseMapper.MapOrderResponse(orderDoc);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Equal(doc.Contacts.Email, response.DataLists.ContactInfoList.FirstOrDefault()?.EmailAddress.FirstOrDefault()?.EmailAddressText);
            }
        }


        [Fact]
        public void OrderInstructions_ResponseMapper_Return_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            orderDoc.DataList.OrderInstructions = new List<OrderInstructions> {
                new OrderInstructions { ItemCode = Constants.Application.SSRGSTE, ItemType = OrderInstructions.ItemTypes.SSREnum, ItemText = "test/check@test.com" },
                new OrderInstructions { ItemCode = Constants.Application.SSRGSTA, ItemType = OrderInstructions.ItemTypes.SSREnum, ItemText = "test1/test2/test3/test4/test5/12345" },
                new OrderInstructions { ItemCode = Constants.Application.SSRGSTN, ItemType = OrderInstructions.ItemTypes.SSREnum, ItemText = "test1/test2/test3" },
                new OrderInstructions { ItemCode = Constants.Application.SSRGSTP, ItemType = OrderInstructions.ItemTypes.SSREnum, ItemText = "test1/test2/test3" }
            };

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Equal(doc.Contacts.Email, response.DataLists.ContactInfoList.FirstOrDefault()?.EmailAddress.FirstOrDefault()?.EmailAddressText);
            }
        }

        [Fact]
        public void TraveldocInfo_ResponseMapper_For5PAX_Return_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.IdentityDocID == doc.TravelDocuments.First().DocumentNumber));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.CitizenshipCountryCode == doc.TravelDocuments.First().CountryOfResidence));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.ExpiryDate == Convert.ToDateTime(doc.TravelDocuments.First().ExpirationDate)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.GivenName.First() == doc.TravelDocuments.First().PersonName.FirstName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.MiddleName.First() == doc.TravelDocuments.First().PersonName.MiddleName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Surname == doc.TravelDocuments.First().PersonName.LastName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.TitleName == doc.TravelDocuments.First().PersonName.Title));

            }
        }

        [Fact]
        public void VisaInfo_ResponseMapper_For5PAX_Return_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.ExpiryDate == Convert.ToDateTime(doc.TravelDocuments.First().ExpirationDate))));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.HostCountryCode == doc.TravelDocuments.First().CountryOfResidence)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.IssuingCountryCode == doc.TravelDocuments.First().CountryOfIssue)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.IssueDate == Convert.ToDateTime(doc.TravelDocuments.First().IssueDate))));
            }
        }

        [Fact]
        public void PaxDetails_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Birthdate == Convert.ToDateTime(doc.DateOfBirth));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.GivenName.First() == doc.FirstName);
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Surname == doc.LastName);
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Surname == doc.LastName);
            }
        }

        [Fact]
        public void PaxSegment_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.SegmentDetails)
            {
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => (x.CabinTypeAssociationChoice.Item as CabinTypeType)?.CabinTypeCode == doc.CabinType.CabinCode);
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => (x.CabinTypeAssociationChoice.Item as CabinTypeType)?.CabinTypeName == doc.CabinType.CabinName);
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => x.MarketingCarrierRBD_Code == doc.ClassOfService);
            }
        }

        [Fact]
        public void OriginDestinationandJourney_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.OriginDestinationAssociations)
            {
                Assert.Contains(response.DataLists.OriginDestList.Where(x => x.OriginDestID == doc.OdLabel), x => x.DestCode == doc.OriginDestination.DestinationAirport);
                Assert.Contains(response.DataLists.OriginDestList.Where(x => x.OriginDestID == doc.OdLabel), x => x.OriginCode == doc.OriginDestination.OriginAirport);
                Assert.Equal(orderDoc.DataList.Journey.First().JourneyId, response.DataLists.PaxJourneyList.First().PaxJourneyID);
            }
        }

        [Fact]
        public void OperatingLegDetails_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.SegmentDetails)
            {
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.CarrierDesigCode == doc.MarketingCarrier);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.MarketingCarrierFlightNumberText == doc.MarketingFlightNumber);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Arrival.IATA_LocationCode == doc.DestinationAirportCode);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Dep.AircraftScheduledDateTime == Convert.ToDateTime(doc.ScheduledDepartureTime.LocalDateTime));
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Dep.IATA_LocationCode == doc.OriginAirportCode);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Arrival.AircraftScheduledDateTime == Convert.ToDateTime(doc.ScheduledArrivalTime.LocalDateTime));
            }
        }

        [Fact]
        public void BaggageAllowance_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.BaggageAllowances)
            {
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => x.TypeCode == doc.BaggageAllowanceTypeCode);
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => x.TypeCode == doc.BaggageAllowanceTypeCode);
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => doc.FreeBaggageAllowance?.AllowedBagCnt.HasValue == false || x.PieceAllowance.TotalQty == doc.FreeBaggageAllowance?.AllowedBagCnt);
            }
        }

        [Fact]
        public void Priceclass_ResponseMapper_For4PAX_Oneway_Tests()
        {

            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\OrderDocResponse4Pax");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.PriceClass)
            {
                Assert.Contains(response.DataLists.PriceClassList.Where(x => x.PriceClassID.Replace("PC", "") == doc.Id.Replace("PC", "")), x => x.FareBasisCode == doc.FareBasisCode);
                Assert.Contains(response.DataLists.PriceClassList.Where(x => x.PriceClassID.Replace("PC", "") == doc.Id.Replace("PC", "")), x => x.Name == doc.ProductName);
            }
        }

        [Fact]
        public void ContactInfo_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Equal(doc.Contacts.Email, response.DataLists.ContactInfoList.FirstOrDefault()?.EmailAddress.FirstOrDefault()?.EmailAddressText);
                Assert.Equal(doc.Contacts.Phone, response.DataLists.ContactInfoList.FirstOrDefault()?.Phone.FirstOrDefault()?.PhoneNumber);
                Assert.Equal(doc.Contacts.CountryDailingCode, response.DataLists.ContactInfoList.FirstOrDefault()?.Phone.FirstOrDefault()?.CountryDialingCode);
            }
        }

        [Fact]
        public void RedressInfo_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\SabreOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.FirstOrDefault().IssuingCountryCode == doc.TravelDocuments.First(x => x.DocumentType == "AEA").CountryOfIssue);

            }
        }

        [Fact]
        public void TraveldocInfo_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.IdentityDocID == doc.TravelDocuments.First().DocumentNumber));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.CitizenshipCountryCode == doc.TravelDocuments.First().CountryOfResidence));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.ExpiryDate == Convert.ToDateTime(doc.TravelDocuments.First().ExpirationDate)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.GivenName.First() == doc.TravelDocuments.First().PersonName.FirstName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.MiddleName.First() == doc.TravelDocuments.First().PersonName.MiddleName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Surname == doc.TravelDocuments.First().PersonName.LastName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.TitleName == doc.TravelDocuments.First().PersonName.Title));

            }
        }

        [Fact]
        public void VisaInfo_ResponseMapper_For4PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.ExpiryDate == Convert.ToDateTime(doc.TravelDocuments.First().ExpirationDate))));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.HostCountryCode == doc.TravelDocuments.First().CountryOfResidence)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.IssuingCountryCode == doc.TravelDocuments.First().CountryOfIssue)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.IssueDate == Convert.ToDateTime(doc.TravelDocuments.First().IssueDate))));
            }
        }

        [Fact]
        public void PaxDetails_ResponseMapper_For5PAX_Oneway_Tests()
        {

            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Birthdate == Convert.ToDateTime(doc.DateOfBirth));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.GivenName.First() == doc.FirstName);
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Surname == doc.LastName);
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.Individual.Surname == doc.LastName);
            }
        }

        [Fact]
        public void PaxSegment_ResponseMapper_For5PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.SegmentDetails)
            {
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => (x.CabinTypeAssociationChoice.Item as CabinTypeType)?.CabinTypeCode == doc.CabinType.CabinCode);
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => (x.CabinTypeAssociationChoice.Item as CabinTypeType)?.CabinTypeName == doc.CabinType.CabinName);
                Assert.Contains(response.DataLists.PaxSegmentList.Where(x => x.PaxSegmentID == doc.SegmentId.Replace("F", "")), x => x.MarketingCarrierRBD_Code == doc.ClassOfService);
            }
        }

        [Fact]
        public void OriginDestinationandJourney_ResponseMapper_For5PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.OriginDestinationAssociations)
            {
                Assert.Contains(response.DataLists.OriginDestList.Where(x => x.OriginDestID == doc.OdLabel), x => x.DestCode == doc.OriginDestination.DestinationAirport);
                Assert.Contains(response.DataLists.OriginDestList.Where(x => x.OriginDestID == doc.OdLabel), x => x.OriginCode == doc.OriginDestination.OriginAirport);
                Assert.Equal(orderDoc.DataList.Journey.First().JourneyId, response.DataLists.PaxJourneyList.First().PaxJourneyID);
            }
        }

        [Fact]
        public void OperatingLegDetails_ResponseMapper_For5PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.SegmentDetails)
            {
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.CarrierDesigCode == doc.MarketingCarrier);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.MarketingCarrierFlightNumberText == doc.MarketingFlightNumber);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Arrival.IATA_LocationCode == doc.DestinationAirportCode);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Dep.AircraftScheduledDateTime == Convert.ToDateTime(doc.ScheduledDepartureTime.LocalDateTime));
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Dep.IATA_LocationCode == doc.OriginAirportCode);
                Assert.Contains(response.DataLists.DatedMarketingSegmentList.Where(x => x.DatedMarketingSegmentId.Replace("DMS", "") == doc.SegmentId.Replace("SEG", "")), x => x.Arrival.AircraftScheduledDateTime == Convert.ToDateTime(doc.ScheduledArrivalTime.LocalDateTime));
            }
        }

        [Fact]
        public void BaggageAllowance_ResponseMapper_For5PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.BaggageAllowances)
            {
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => x.TypeCode == doc.BaggageAllowanceTypeCode);
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => x.TypeCode == doc.BaggageAllowanceTypeCode);
                Assert.Contains(response.DataLists.BaggageAllowanceList.Where(x => x.BaggageAllowanceID.Replace("BAGA", "") == doc.BaggageAllowanceId.Replace("BAG", "")), x => doc.FreeBaggageAllowance?.AllowedBagCnt.HasValue == false || x.PieceAllowance.TotalQty == doc.FreeBaggageAllowance?.AllowedBagCnt);
            }
        }

        [Fact]
        public void Priceclass_ResponseMapper_For5PAX_Oneway_Tests()
        {

            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.PriceClass)
            {
                Assert.Contains(response.DataLists.PriceClassList.Where(x => x.PriceClassID.Replace("PC", "") == doc.Id.Replace("PC", "")), x => x.FareBasisCode == doc.FareBasisCode);
                Assert.Contains(response.DataLists.PriceClassList.Where(x => x.PriceClassID.Replace("PC", "") == doc.Id.Replace("PC", "")), x => x.Name == doc.ProductName);
            }
        }

        [Fact]
        public void ContactInfo_ResponseMapper_For5PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Equal(doc.Contacts.Email, response.DataLists.ContactInfoList.FirstOrDefault()?.EmailAddress.FirstOrDefault()?.EmailAddressText);
                Assert.Equal(doc.Contacts.Phone, response.DataLists.ContactInfoList.FirstOrDefault()?.Phone.FirstOrDefault()?.PhoneNumber);
                Assert.Equal(doc.Contacts.CountryDailingCode, response.DataLists.ContactInfoList.FirstOrDefault()?.Phone.FirstOrDefault()?.CountryDialingCode);
            }
        }

        [Fact]
        public void TraveldocInfo_ResponseMapper_For5PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.IdentityDocID == doc.TravelDocuments.First().DocumentNumber));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.CitizenshipCountryCode == doc.TravelDocuments.First().CountryOfResidence));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.ExpiryDate == Convert.ToDateTime(doc.TravelDocuments.First().ExpirationDate)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.GivenName.First() == doc.TravelDocuments.First().PersonName.FirstName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.MiddleName.First() == doc.TravelDocuments.First().PersonName.MiddleName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Surname == doc.TravelDocuments.First().PersonName.LastName));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.TitleName == doc.TravelDocuments.First().PersonName.Title));

            }
        }

        [Fact]
        public void VisaInfo_ResponseMapper_For5PAX_Oneway_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            foreach (var doc in orderDoc.DataList.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.ExpiryDate == Convert.ToDateTime(doc.TravelDocuments.First().ExpirationDate))));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.HostCountryCode == doc.TravelDocuments.First().CountryOfResidence)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.IssuingCountryCode == doc.TravelDocuments.First().CountryOfIssue)));
                Assert.Contains(response.DataLists.PaxList.Where(x => x.PaxID == doc.CustId), x => x.IdentityDoc.Any(x => x.Visa.Any(x => x.IssueDate == Convert.ToDateTime(doc.TravelDocuments.First().IssueDate))));
            }
        }

        [Fact]
        public void TicketInfo_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderRetrieve\\NewOrderDoc");

            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            var paymentFunctions = new PaymentFunctionsOrderType[] { result.PaymentFunctions.First() };

            Assert.NotNull(response?.TicketDocInfo.First());
            Assert.Equal("ADULT_1", response.TicketDocInfo.First().PaxRefID);
            Assert.Equal(1, response.TicketDocInfo.First().BookletQty);
            Assert.Equal("T", response.TicketDocInfo.First().Ticket.First().TicketDocTypeCode);
            Assert.Equal("T", response.TicketDocInfo.Last().Ticket.First().TicketDocTypeCode);
            Assert.Equal("INFANT_1", response.TicketDocInfo.Last().PaxRefID);
            Assert.Equal(5, response.TicketDocInfo.Count());
            Assert.Equal("80704271", paymentFunctions.First().PaymentProcessingSummary.PaymentID);
        }


        [Fact]
        public void FareTypeCodeInfo_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderCreate\\FareTypeOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First());
            Assert.Equal("Marine", response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First().FareTypeCode);
            Assert.Equal("758", response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First().NegotiatedCode);
            Assert.Equal("IY9VKDJA", response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First().FareBasisCode);
            Assert.Equal("123456", response.DataLists.ContactInfoList.First().Phone.First().PhoneNumber);
            Assert.Equal("44", response.DataLists.ContactInfoList.First().Phone.First().CountryDialingCode);
            Assert.Equal("HOME", response.DataLists.ContactInfoList.First().Phone.First().ContactTypeText);
        }

        [Fact]

        public void EasyPayPaymentFuction_ResponseMapper_Test()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderCancelEasyPay");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, false, false, false, false, null, null, false, true, "095968");
            var paymentResponse = ((PaymentFunctionsOrderType[])result.PaymentFunctions).First();

            Assert.NotNull(paymentResponse);
            Assert.Equal("VS932TDVJPB26", paymentResponse.OrderAssociation.OrderRefID);
            Assert.Equal("176768589045197724", paymentResponse.PaymentProcessingSummary.PaymentID);

        }


        [Fact]
        public void OrderChange_DiffPrice_ResponseMapper_Test()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderChangeOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, false, true, false, false, null, null);
            var response = ((OrderViewResponseType[])result.Items).First();
            var orderItem = response.Order.SelectMany(o => o.OrderItem).FirstOrDefault(i => i.PriceDifferential != null);
            var diff = orderItem?.PriceDifferential;
            var taxSummary = diff.DiffPrice.Price.TaxSummary.First();
            var taxes = taxSummary.Tax;

            Assert.NotNull(diff);
            Assert.NotNull(diff.DiffPrice.Price.TaxSummary);
            Assert.Equal(8, taxes.Length);

            Assert.Equal("GB", taxes[0].TaxCode);
            Assert.Equal("United Kingdom Air Passenger Duty APD", taxes[0].DescText);
            Assert.Equal("0.00", taxes[0].Amount.Value.ToString());

            Assert.Equal("UB", taxes[1].TaxCode);
            Assert.Equal("United Kingdom Passenger Service Charge Departures", taxes[1].DescText);
            Assert.Equal("0.00", taxes[1].Amount.Value.ToString());

            Assert.Equal("US", taxes[2].TaxCode);
            Assert.Equal("US International Arrival Tax", taxes[2].DescText);
            Assert.Equal("0.00", taxes[2].Amount.Value.ToString());

            Assert.Equal("XA", taxes[3].TaxCode);
            Assert.Equal("United States APHIS Passenger Fee Passengers", taxes[3].DescText);
            Assert.Equal("0.00", taxes[3].Amount.Value.ToString());

            Assert.Equal("XY", taxes[4].TaxCode);
            Assert.Equal("United States Immigration User Fee", taxes[4].DescText);
            Assert.Equal("0.00", taxes[4].Amount.Value.ToString());

            Assert.Equal("YC", taxes[5].TaxCode);
            Assert.Equal("United States Customs User Fee", taxes[5].DescText);
            Assert.Equal("0.00", taxes[5].Amount.Value.ToString());

            Assert.Equal("YQ", taxes[6].TaxCode);
            Assert.Equal("VS YQ surcharge", taxes[6].DescText);
            Assert.Equal("0.00", taxes[6].Amount.Value.ToString());

            Assert.Equal("YR", taxes[7].TaxCode);
            Assert.Equal("VS YR surcharge", taxes[7].DescText);
            Assert.Equal("0.00", taxes[7].Amount.Value.ToString());
        }

        [Fact]
        public void OrderChange_DisruptionPriceDifferential_ResponseMapper_Test()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderChangeOrderDoc");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, false, true, false, false, null, null, isReshopDisruption: true);
            var response = ((OrderViewResponseType[])result.Items).First();
            var priceDifferential = response.Order.SelectMany(order => order.OrderItem)
                .First(orderItem => orderItem.PriceDifferential != null).PriceDifferential;

            Assert.Equal(DifferentialTypeCodeType.EvenExchange, priceDifferential.DifferentialTypeCode);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.BaseAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.TotalAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.DueByAirlineAmount.Value);
            Assert.Equal(0.00m, priceDifferential.DiffPrice.Price.DueToAirlineAmount.Value);
            Assert.All(priceDifferential.DiffPrice.Price.TaxSummary, taxSummary =>
            {
                Assert.Equal(0.00m, taxSummary.TotalTaxAmount.Value);
                Assert.All(taxSummary.Tax, tax => Assert.Equal(0.00m, tax.Amount.Value));
            });
            Assert.Equal(0.00m, priceDifferential.NewPrice.Price.BaseAmount.Value);
            Assert.Equal(0.00m, priceDifferential.NewPrice.Price.TotalAmount.Value);
            Assert.All(priceDifferential.NewPrice.Price.TaxSummary, taxSummary =>
            {
                Assert.Equal(0.00m, taxSummary.TotalTaxAmount.Value);
                Assert.Equal(0.00m, taxSummary.TotalRefundableTaxAmount.Value);
                Assert.All(taxSummary.Tax, tax => Assert.Equal(0.00m, tax.Amount.Value));
            });
            Assert.NotEqual(0.00m, priceDifferential.OldPrice.Price.TotalAmount.Value);
        }

        [Fact]
        public void OrderRetrieve_Warning_Test()
        {
            var orderDoc = new OrderDocument()
            {
                Warnings = [
                    new()
                    {
                        Code = "186"
                    }
                ]
            };

            var response = OrderRetrieveValidation.SSRWarnings(orderDoc, _hostEnvironment);

            Assert.Contains("not applicable", response.First().ShortText);
        }

        [Fact]
        public void OrderChangePartialFlownScenarios_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderDocResponsePartialFlown");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, false, true);
            var response = ((OrderViewResponseType[])result.Items).First();

            Assert.NotNull(response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First());
            Assert.NotNull(response.TicketDocInfo.First().OriginalIssueInfo.TicketNumber);
            Assert.Equal("9322520394703", response.TicketDocInfo.First().OriginalIssueInfo.TicketNumber);
        }

        [Fact]
        public void OrderChangeArnkPartialFlownScenarios_ResponseMapper_Tests()
        {
            var orderDoc = TestDataProvider.GetTestData<OrderDocument>("NDCOrder\\OrderChange\\OrderDocResponsePartialFlownARNK");
            var result = OrderResponseMapper.MapOrderResponse(orderDoc, null, false, true);
            var response = ((OrderViewResponseType[])result.Items).First();
            var changedFlightOrderItem = response.Order.Single().OrderItem.Single(orderItem => orderItem.OrderItemID == "VS932OVBLGC26-1A");
            var currentTicket = response.TicketDocInfo.Single();
            var returnOriginDestination = response.DataLists.OriginDestList.Single(originDestination => originDestination.OriginDestID == "OD2");

            Assert.NotNull(response.Order.First().OrderItem.First().FareDetail.First().FareComponent.First());
            Assert.NotNull(response.TicketDocInfo.First().OriginalIssueInfo.TicketNumber);
            Assert.Equal("9322520015917", response.TicketDocInfo.First().OriginalIssueInfo.TicketNumber);
            Assert.Equal("VS932OVBLGC26-1", changedFlightOrderItem.ExistingOrderItemID);
            Assert.Equal(new[] { "SEG2OLD", "SEG1", "SEG2", "SEG3" }, response.DataLists.PaxSegmentList.Select(segment => segment.PaxSegmentID));
            Assert.Equal("EWR", returnOriginDestination.OriginCode);
            Assert.Equal("LHR", returnOriginDestination.DestCode);
            Assert.Contains(changedFlightOrderItem.FareDetail.Single().FareComponent, fareComponent => fareComponent.FareBasisCode == "EY30KDLA" && fareComponent.PaxSegmentRefID.SequenceEqual(new[] { "SEG2", "SEG3" }));
            Assert.Equal(DifferentialTypeCodeType.AddCol, changedFlightOrderItem.PriceDifferential.DifferentialTypeCode);
            Assert.Equal(231.10m, changedFlightOrderItem.PriceDifferential.DiffPrice.Price.DueToAirlineAmount.Value);
            Assert.Equal("GBP", changedFlightOrderItem.PriceDifferential.DiffPrice.Price.DueToAirlineAmount.CurCode);
            Assert.Equal("9322520018913", currentTicket.Ticket.Single().TicketNumber);
            Assert.Equal(new[] { "SEG2", "SEG3" }, currentTicket.Ticket.Single().Coupon.Select(coupon => coupon.SoldAirlineInfo.Single()));
            Assert.All(currentTicket.Ticket.Single().Coupon, coupon =>
            {
                Assert.Equal("EY30KDLA", coupon.FareBasisCode);
                Assert.Equal("OPE", coupon.CouponStatusCode);
            });
        }
    }
}
