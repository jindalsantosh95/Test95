using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Mapper.Order;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests
{
    public class GdsOrderResponseMapperTests
    {
        private readonly GdsOrderResponseMapper _mapper;

        public GdsOrderResponseMapperTests()
        {
            _mapper = new GdsOrderResponseMapper();
        }

        [Fact]
        public void Mapper_ShouldReturnProperResponse_MultiCity2ADTScenario()
        {
            //Arrange
            var orderSearchResponse = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponse_MultiCity_2ADT");

            //Act
            var result = _mapper.MapOrderResponseForGDS(orderSearchResponse);

            //Assert
            var response = (result.Items as OrderViewResponseType[])?.FirstOrDefault();
            Assert.NotNull(response);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC,response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(1, response.Order.FirstOrDefault()?.OrderItem.Length);
            Assert.Equal(orderSearchResponse.SegmentDetails.Count()*orderSearchResponse.CustomerDetails.Count(),response.Order.FirstOrDefault()?.OrderItem.First().Service.Length);

            Assert.Equal(orderSearchResponse.OriginDestinationAssociations.Count(),response.DataLists.OriginDestList.Length);
            Assert.Equal(orderSearchResponse.OriginDestinationAssociations.First().OriginDestination.OriginAirport, response.DataLists.OriginDestList.First().OriginCode);
            Assert.Equal(orderSearchResponse.OriginDestinationAssociations.Last().OriginDestination.DestinationAirport, response.DataLists.OriginDestList.Last().DestCode);

            Assert.Equal(orderSearchResponse.CustomerDetails.Count() , response.DataLists.PaxList.Length);
            Assert.Equal(orderSearchResponse.CustomerDetails.First().FirstName, response.DataLists.PaxList.First().Individual.GivenName.First());
            Assert.Equal(orderSearchResponse.CustomerDetails.First().LastName, response.DataLists.PaxList.First().Individual.Surname);

            Assert.Equal(orderSearchResponse.SegmentDetails.Count() , response.DataLists.PaxSegmentList.Length);
            Assert.Equal(orderSearchResponse.SegmentDetails.First().ClassOfService , response.DataLists.PaxSegmentList.First().MarketingCarrierRBD_Code);
            Assert.Equal(orderSearchResponse.SegmentDetails.First().CabinType.CabinName, (response.DataLists.PaxSegmentList.First().CabinTypeAssociationChoice.Item as CabinTypeType)?.CabinTypeName);
            Assert.Equal(orderSearchResponse.SegmentDetails.First().CabinType.CabinCode, (response.DataLists.PaxSegmentList.First().CabinTypeAssociationChoice.Item as CabinTypeType)?.CabinTypeCode); 
            Assert.Contains(response.DataLists.PaxSegmentList, x => x.PaxSegmentID.StartsWith(Constants.Application.SEG));

            foreach ( var seg in orderSearchResponse.SegmentDetails.GroupBy(x => x.OriginDestinationRefId))
            {
                var paxJourneyRef = response.DataLists.OriginDestList.First(x => x.OriginDestID == seg.First().OriginDestinationRefId).PaxJourneyRefID;
                var segRef = response.DataLists.PaxJourneyList.Where(x => paxJourneyRef.Contains(x.PaxJourneyID)).SelectMany(x => x.PaxSegmentRefID);
                var datedMarketingSegRef = response.DataLists.PaxSegmentList.Where(x => segRef.Contains(x.PaxSegmentID)).Select(x => x.DatedMarketingSegmentRefId);
                var datedOperatingSegRef = response.DataLists.DatedMarketingSegmentList.Where(x=> datedMarketingSegRef.Contains(x.DatedMarketingSegmentId)).Select(x => x.DatedOperatingSegmentRefId);
                var flights = response.DataLists.DatedOperatingSegmentList.Where(x => datedOperatingSegRef.Contains(x.DatedOperatingSegmentId)).Select(x => x.OperatingCarrierFlightNumberText);

                Assert.True(flights.Intersect(seg.Select(x => x.MarketingFlightNumber)).Count() == flights.Count());
                Assert.True(flights.SequenceEqual(seg.Select(x => x.MarketingFlightNumber)));
            }

            Assert.Equal(orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs).Count() , response.DataLists.DatedOperatingLegList.Length);

            foreach(var leg in orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs))
            {
                var datedOperatingLeg = response.DataLists.DatedOperatingLegList.FirstOrDefault(x => x.Arrival.IATA_LocationCode == leg.Destination_Airport_Code && x.Dep.IATA_LocationCode == leg.OriginAirportCode);

                Assert.NotNull(datedOperatingLeg);
                Assert.Equal(Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Dep.AircraftScheduledDateTime));
                Assert.Equal(Convert.ToDateTime(leg.ScheduledArrivalTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Arrival.AircraftScheduledDateTime));
            }

            Assert.Equal(Convert.ToDecimal(orderSearchResponse.TotalPrice.MonetaryAmount), response.Order.First().TotalPrice.TotalAmount.Value);
        }

        [Fact]
        public void Mapper_ShouldReturnProperResponse_Return3ADT1CHD()
        {
            //Arrange
            var orderSearchResponse = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponseGDS_Return_3ADT1CHD");

            //Act
            var result = _mapper.MapOrderResponseForGDS(orderSearchResponse);

            //Assert
            var response = (result.Items as OrderViewResponseType[])?.FirstOrDefault();
            Assert.NotNull(response);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC, response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC, response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(2, response.Order.FirstOrDefault()?.OrderItem.Length);
            Assert.Equal(orderSearchResponse.SegmentDetails.Count() * orderSearchResponse.CustomerDetails.Count(), response.Order.FirstOrDefault()?.OrderItem.SelectMany(x => x.Service).Count());

            Assert.Equal(orderSearchResponse.OriginDestinationAssociations.Count(), response.DataLists.OriginDestList.Length);

            foreach (var (ond, index) in orderSearchResponse.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
            {
                Assert.Equal(ond.OriginDestination.OriginAirport, response.DataLists.OriginDestList[index].OriginCode);
                Assert.Equal(ond.OriginDestination.DestinationAirport, response.DataLists.OriginDestList[index].DestCode);
            }

            Assert.Equal(orderSearchResponse.CustomerDetails.Count(), response.DataLists.PaxList.Length);

            foreach(var cust in orderSearchResponse.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.GivenName.First() == cust.FirstName);
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.Surname == cust.LastName);
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.Birthdate == Convert.ToDateTime(cust.DateOfBirth));
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.GenderCode.ToString() == cust.Gender);

                Assert.True(cust.TravelDocuments.Select(x => x.DocumentNumber).SequenceEqual(response.DataLists.PaxList.First(x => x.Individual.GivenName.First() == cust.FirstName).IdentityDoc.Select(x => x.IdentityDocID)));
            }

            foreach (var seg in orderSearchResponse.SegmentDetails.GroupBy(x => x.OriginDestinationRefId))
            {
                var paxJourneyRef = response.DataLists.OriginDestList.First(x => x.OriginDestID == seg.First().OriginDestinationRefId).PaxJourneyRefID;
                var segRef = response.DataLists.PaxJourneyList.Where(x => paxJourneyRef.Contains(x.PaxJourneyID)).SelectMany(x => x.PaxSegmentRefID);
                var datedMarketingSegRef = response.DataLists.PaxSegmentList.Where(x => segRef.Contains(x.PaxSegmentID)).Select(x => x.DatedMarketingSegmentRefId);
                var datedOperatingSegRef = response.DataLists.DatedMarketingSegmentList.Where(x => datedMarketingSegRef.Contains(x.DatedMarketingSegmentId)).Select(x => x.DatedOperatingSegmentRefId);
                var flights = response.DataLists.DatedOperatingSegmentList.Where(x => datedOperatingSegRef.Contains(x.DatedOperatingSegmentId)).Select(x => x.OperatingCarrierFlightNumberText);

                Assert.True(flights.Intersect(seg.Select(x => x.MarketingFlightNumber)).Count() == flights.Count());
                Assert.True(flights.SequenceEqual(seg.Select(x => x.MarketingFlightNumber)));
            }

            Assert.Equal(orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs).Count(), response.DataLists.DatedOperatingLegList.Length);

            foreach (var leg in orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs))
            {
                var datedOperatingLeg = response.DataLists.DatedOperatingLegList.FirstOrDefault(x => x.Arrival.IATA_LocationCode == leg.Destination_Airport_Code && x.Dep.IATA_LocationCode == leg.OriginAirportCode);

                Assert.NotNull(datedOperatingLeg);
                Assert.Equal(Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Dep.AircraftScheduledDateTime));
                Assert.Equal(Convert.ToDateTime(leg.ScheduledArrivalTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Arrival.AircraftScheduledDateTime));
            }

            Assert.Equal(Convert.ToDecimal(orderSearchResponse.TotalPrice.MonetaryAmount), response.Order.First().TotalPrice.TotalAmount.Value);
        }

        [Fact]
        public void Mapper_ShouldReturnProperResponse_Return6ADT1INF()
        {
            //Arrange
            var orderSearchResponse = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponseGDS_Return_6ADT1INF");

            //Act
            var result = _mapper.MapOrderResponseForGDS(orderSearchResponse);

            //Assert
            var response = (result.Items as OrderViewResponseType[])?.FirstOrDefault();
            Assert.NotNull(response);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC, response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC, response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(2, response.Order.FirstOrDefault()?.OrderItem.Length);
            Assert.Equal(orderSearchResponse.SegmentDetails.Count() * orderSearchResponse.CustomerDetails.Count(), response.Order.FirstOrDefault()?.OrderItem.SelectMany(x => x.Service).Count());

            Assert.Equal(orderSearchResponse.OriginDestinationAssociations.Count(), response.DataLists.OriginDestList.Length);

            foreach (var (ond, index) in orderSearchResponse.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
            {
                Assert.Equal(ond.OriginDestination.OriginAirport, response.DataLists.OriginDestList[index].OriginCode);
                Assert.Equal(ond.OriginDestination.DestinationAirport, response.DataLists.OriginDestList[index].DestCode);
            }

            Assert.Equal(orderSearchResponse.CustomerDetails.Count(), response.DataLists.PaxList.Length);

            foreach (var cust in orderSearchResponse.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.GivenName.First() == cust.FirstName);
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.Surname == cust.LastName);
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.Birthdate == Convert.ToDateTime(cust.DateOfBirth));
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.GenderCode.ToString() == cust.Gender);
            }

            foreach (var seg in orderSearchResponse.SegmentDetails.GroupBy(x => x.OriginDestinationRefId))
            {
                var paxJourneyRef = response.DataLists.OriginDestList.First(x => x.OriginDestID == seg.First().OriginDestinationRefId).PaxJourneyRefID;
                var segRef = response.DataLists.PaxJourneyList.Where(x => paxJourneyRef.Contains(x.PaxJourneyID)).SelectMany(x => x.PaxSegmentRefID);
                var datedMarketingSegRef = response.DataLists.PaxSegmentList.Where(x => segRef.Contains(x.PaxSegmentID)).Select(x => x.DatedMarketingSegmentRefId);
                var datedOperatingSegRef = response.DataLists.DatedMarketingSegmentList.Where(x => datedMarketingSegRef.Contains(x.DatedMarketingSegmentId)).Select(x => x.DatedOperatingSegmentRefId);
                var flights = response.DataLists.DatedOperatingSegmentList.Where(x => datedOperatingSegRef.Contains(x.DatedOperatingSegmentId)).Select(x => x.OperatingCarrierFlightNumberText);

                Assert.True(flights.Intersect(seg.Select(x => x.MarketingFlightNumber)).Count() == flights.Count());
                Assert.True(flights.SequenceEqual(seg.Select(x => x.MarketingFlightNumber)));
            }

            Assert.Equal(orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs).Count(), response.DataLists.DatedOperatingLegList.Length);

            foreach (var leg in orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs))
            {
                var datedOperatingLeg = response.DataLists.DatedOperatingLegList.FirstOrDefault(x => x.Arrival.IATA_LocationCode == leg.Destination_Airport_Code && x.Dep.IATA_LocationCode == leg.OriginAirportCode);

                Assert.NotNull(datedOperatingLeg);
                Assert.Equal(Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Dep.AircraftScheduledDateTime));
                Assert.Equal(Convert.ToDateTime(leg.ScheduledArrivalTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Arrival.AircraftScheduledDateTime));
            }

            Assert.Equal(Convert.ToDecimal(orderSearchResponse.TotalPrice.MonetaryAmount), response.Order.First().TotalPrice.TotalAmount.Value);
        }

        [Fact]
        public void Mapper_ShouldReturnProperResponse_OneWay1ADT1GBE_WithSeat()
        {
            //Arrange
            var orderSearchResponse = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponseGDSWithSeat_OneWay_1ADT1GBE");

            //Act
            var result = _mapper.MapOrderResponseForGDS(orderSearchResponse);

            //Assert
            var response = (result.Items as OrderViewResponseType[])?.FirstOrDefault();
            Assert.NotNull(response);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC, response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC, response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(3, response.Order.FirstOrDefault()?.OrderItem.Length);

            Assert.Equal(orderSearchResponse.OriginDestinationAssociations.Count(), response.DataLists.OriginDestList.Length);

            foreach (var (ond, index) in orderSearchResponse.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
            {
                Assert.Equal(ond.OriginDestination.OriginAirport, response.DataLists.OriginDestList[index].OriginCode);
                Assert.Equal(ond.OriginDestination.DestinationAirport, response.DataLists.OriginDestList[index].DestCode);
            }

            Assert.Equal(orderSearchResponse.CustomerDetails.Count(), response.DataLists.PaxList.Length);

            foreach (var cust in orderSearchResponse.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.GivenName.First() == cust.FirstName);
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.Surname == cust.LastName);
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.Birthdate == Convert.ToDateTime(cust.DateOfBirth));
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.GenderCode.ToString() == cust.Gender);

                Assert.True(cust.TravelDocuments.Select(x => x.DocumentNumber).SequenceEqual(response.DataLists.PaxList.First(x => x.Individual.GivenName.First() == cust.FirstName).IdentityDoc.Select(x => x.IdentityDocID)));
            }

            foreach (var seg in orderSearchResponse.SegmentDetails.GroupBy(x => x.OriginDestinationRefId))
            {
                var paxJourneyRef = response.DataLists.OriginDestList.First(x => x.OriginDestID == seg.First().OriginDestinationRefId).PaxJourneyRefID;
                var segRef = response.DataLists.PaxJourneyList.Where(x => paxJourneyRef.Contains(x.PaxJourneyID)).SelectMany(x => x.PaxSegmentRefID);
                var datedMarketingSegRef = response.DataLists.PaxSegmentList.Where(x => segRef.Contains(x.PaxSegmentID)).Select(x => x.DatedMarketingSegmentRefId);
                var datedOperatingSegRef = response.DataLists.DatedMarketingSegmentList.Where(x => datedMarketingSegRef.Contains(x.DatedMarketingSegmentId)).Select(x => x.DatedOperatingSegmentRefId);
                var flights = response.DataLists.DatedOperatingSegmentList.Where(x => datedOperatingSegRef.Contains(x.DatedOperatingSegmentId)).Select(x => x.OperatingCarrierFlightNumberText);

                Assert.True(flights.Intersect(seg.Select(x => x.MarketingFlightNumber)).Count() == flights.Count());
                Assert.True(flights.SequenceEqual(seg.Select(x => x.MarketingFlightNumber)));
            }

            Assert.Equal(orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs).Count(), response.DataLists.DatedOperatingLegList.Length);

            foreach (var leg in orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs))
            {
                var datedOperatingLeg = response.DataLists.DatedOperatingLegList.FirstOrDefault(x => x.Arrival.IATA_LocationCode == leg.Destination_Airport_Code && x.Dep.IATA_LocationCode == leg.OriginAirportCode);

                Assert.NotNull(datedOperatingLeg);
                Assert.Equal(Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Dep.AircraftScheduledDateTime));
                Assert.Equal(Convert.ToDateTime(leg.ScheduledArrivalTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Arrival.AircraftScheduledDateTime));
            }

            Assert.Equal(Convert.ToDecimal(orderSearchResponse.TotalPrice.MonetaryAmount), response.Order.First().TotalPrice.TotalAmount.Value);
            Assert.Equal(orderSearchResponse.OrderItems.Count(), response.Order.First().OrderItem.Length);

            foreach(var (orderItem, index) in orderSearchResponse.OrderItems.Select((orderItem, index) => (orderItem, index)))
            {
                if (orderItem.PriceDetails != null)
                {
                    Assert.Equal(orderItem.PriceDetails.First(x => x.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount, response.Order.First().OrderItem[index].Price.TotalAmount.Value.ToString());
                    Assert.Equal(orderItem.PriceDetails.First(x => x.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.BaseCurrency, response.Order.First().OrderItem[index].Price.TotalAmount.CurCode);
                }

                Assert.Equal(orderSearchResponse.SegmentDetails.Count() , response.Order.First().OrderItem[index].Service.Length);
            }
        }

        [Fact]
        public void Mapper_ShouldReturnProperResponse_Return2ADT1CHD1INF_WithSeat()
        {
            //Arrange
            var orderSearchResponse = TestDataProvider.GetTestData<OrderSearchResponse>("NDCOrder\\OrderSearchResponseGDSWithSeat_Return_2ADT1CHD1INF");

            //Act
            var result = _mapper.MapOrderResponseForGDS(orderSearchResponse);

            //Assert
            var response = (result.Items as OrderViewResponseType[])?.FirstOrDefault();
            Assert.NotNull(response);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC, response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(orderSearchResponse.BookingReferences.RLOC, response.Order.FirstOrDefault()?.OrderID);
            Assert.Equal(orderSearchResponse.OriginDestinationAssociations.Count(), response.DataLists.OriginDestList.Length);

            foreach (var (ond, index) in orderSearchResponse.OriginDestinationAssociations.Select((ond, index) => (ond, index)))
            {
                Assert.Equal(ond.OriginDestination.OriginAirport, response.DataLists.OriginDestList[index].OriginCode);
                Assert.Equal(ond.OriginDestination.DestinationAirport, response.DataLists.OriginDestList[index].DestCode);
            }

            Assert.Equal(orderSearchResponse.CustomerDetails.Count(), response.DataLists.PaxList.Length);

            foreach (var cust in orderSearchResponse.CustomerDetails)
            {
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.GivenName.First() == cust.FirstName);
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.Surname == cust.LastName);
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.Birthdate == Convert.ToDateTime(cust.DateOfBirth));
                Assert.Contains(response.DataLists.PaxList, x => x.Individual.GenderCode.ToString() == cust.Gender);

                Assert.True(cust.TravelDocuments.Select(x => x.DocumentNumber).SequenceEqual(response.DataLists.PaxList.First(x => x.Individual.GivenName.First() == cust.FirstName).IdentityDoc.Select(x => x.IdentityDocID)));
            }

            foreach (var seg in orderSearchResponse.SegmentDetails.GroupBy(x => x.OriginDestinationRefId))
            {
                var paxJourneyRef = response.DataLists.OriginDestList.First(x => x.OriginDestID == seg.First().OriginDestinationRefId).PaxJourneyRefID;
                var segRef = response.DataLists.PaxJourneyList.Where(x => paxJourneyRef.Contains(x.PaxJourneyID)).SelectMany(x => x.PaxSegmentRefID);
                var datedMarketingSegRef = response.DataLists.PaxSegmentList.Where(x => segRef.Contains(x.PaxSegmentID)).Select(x => x.DatedMarketingSegmentRefId);
                var datedOperatingSegRef = response.DataLists.DatedMarketingSegmentList.Where(x => datedMarketingSegRef.Contains(x.DatedMarketingSegmentId)).Select(x => x.DatedOperatingSegmentRefId);
                var flights = response.DataLists.DatedOperatingSegmentList.Where(x => datedOperatingSegRef.Contains(x.DatedOperatingSegmentId)).Select(x => x.OperatingCarrierFlightNumberText);

                Assert.True(flights.Intersect(seg.Select(x => x.MarketingFlightNumber)).Count() == flights.Count());
                Assert.True(flights.SequenceEqual(seg.Select(x => x.MarketingFlightNumber)));
            }

            Assert.Equal(orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs).Count(), response.DataLists.DatedOperatingLegList.Length);

            foreach (var leg in orderSearchResponse.SegmentDetails.SelectMany(x => x.Legs))
            {
                var datedOperatingLeg = response.DataLists.DatedOperatingLegList.FirstOrDefault(x => x.Arrival.IATA_LocationCode == leg.Destination_Airport_Code && x.Dep.IATA_LocationCode == leg.OriginAirportCode);

                Assert.NotNull(datedOperatingLeg);
                Assert.Equal(Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Dep.AircraftScheduledDateTime));
                Assert.Equal(Convert.ToDateTime(leg.ScheduledArrivalTime.LocalDateTime), Convert.ToDateTime(datedOperatingLeg.Arrival.AircraftScheduledDateTime));
            }

            Assert.Equal(Convert.ToDecimal(orderSearchResponse.TotalPrice.MonetaryAmount), response.Order.First().TotalPrice.TotalAmount.Value);
            Assert.Equal(6, response.Order.First().OrderItem.Length);

            foreach (var (orderItem, index) in orderSearchResponse.OrderItems.Select((orderItem, index) => (orderItem, index)))
            {
                if (orderItem.PriceDetails != null)
                {
                    var ptc = response.DataLists.PaxList.FirstOrDefault(x => x.Individual.GivenName.First() == orderSearchResponse.CustomerDetails.First(x => x.CustId == orderItem.ServiceBeneficiaries.First()).FirstName)?.PTC;
                    var samePtcCusts = orderSearchResponse.CustomerDetails.Where(x => response.DataLists.PaxList.Any(y => y.Individual.GivenName.First() == x.FirstName && y.PTC == ptc)).Select(x => x.CustId);
                    var amount = orderSearchResponse.OrderItems.Where(x => samePtcCusts.Any(y => x.ServiceBeneficiaries.First() == y && x.Services.First().ProductOrder.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum)).Sum(x => Convert.ToDecimal(x.PriceDetails.First(x => x.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount));

                    Assert.Equal(amount, response.Order.First().OrderItem.First(x => response.DataLists.PaxList.Any(y => y.PTC == ptc && y.PaxID == x.Service.First().PaxRefID)).Price.TotalAmount.Value);
                    Assert.Equal(orderItem.PriceDetails.First(x => x.PriceComponentType == EAPI.Domain.Virgin.Models.Common.Price.PriceComponentTypes.BasePriceEnum).Amount.BaseCurrency, response.Order.First().OrderItem[index].Price.TotalAmount.CurCode);
                }

            }
        }
    }
}
