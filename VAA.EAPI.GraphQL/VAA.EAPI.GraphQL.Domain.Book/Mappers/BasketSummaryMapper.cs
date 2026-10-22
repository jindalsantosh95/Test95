using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Book.Helpers;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order;
using static VAA.EAPI.GraphQL.Domain.Book.Models.Common.Price;

namespace VAA.EAPI.GraphQL.Domain.Book.Mappers
{
    public class BasketSummaryMapper
    {
        public static BasketSummary MapFlightSummary(OfferResponse offerResponse, Basket basket,
            IEnumerable<AirportRefData> airportData, IEnumerable<AirlineRefData> airlineData, OrderResponseMS orderResponseMS = null,
            IEnumerable<CabinRefData> cabinData = null, IEnumerable<AircraftRefData> airCraftData = null)
        {
            IEnumerable<BasketPerson> peoples = basket.People;
            IEnumerable<FlightPassengerSeatMap> seatMaps = basket.FlightPassengerSeatMap;
            BasketLoyaltyMemberDetails customerProfile = basket.CustomerProfile;
            var voucherRedemptionEnabled = VoucherRedemptionFeatureHelper.IsEnabled();

            var inboundFlights = new List<BasketFlight>();
            var outboundFlights = new List<BasketFlight>();
            var slices = new List<BasketFlight>();

            var price = Convert.ToDouble(offerResponse.Offers.First().SalePrice.Where(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).First().Amount.MonetaryAmount);

            var outboundOdRefId = offerResponse.DataList.FlightSegments.OrderBy(x => x.ScheduledDepartureTime.LocalDateTime).First().OdRefId;
            var outboundFlightSegments = offerResponse.DataList.FlightSegments?.Where(segment => segment.OdRefId == outboundOdRefId);

            string departureDate = DateTime.Parse(offerResponse.DataList.FlightSegments.Min(x => x.ScheduledDepartureTime.LocalDateTime)).ToString("yyyy-MM-dd");

            string returnDate = null;

            outboundFlights.Add(GetBasketFlight(offerResponse, outboundFlightSegments, orderResponseMS, airportData, airlineData, cabinData, airCraftData));

            var inboundOdRefId = offerResponse.DataList.OriginDestinationAssociations.Count() >= 2 ? offerResponse.DataList.FlightSegments.OrderBy(x => x.ScheduledDepartureTime.LocalDateTime).Last().OdRefId : null;
            var inboundFlightSegments = inboundOdRefId != null ? offerResponse.DataList.FlightSegments?.Where(segment => segment.OdRefId == inboundOdRefId) : null;

            if (inboundFlightSegments != null && inboundFlightSegments.Any())
            {
                string inboundCabinName = offerResponse.Offers.First().OfferItems.First().Services.Where(x => x.FlightSegmentsRefIds.First() == inboundFlightSegments.First().SegmentId).First().FareDetails.FareSegments.First().CabinName;
                returnDate = DateTime.Parse(inboundFlightSegments.Last().ScheduledArrivalTime.LocalDateTime).ToString("yyyy-MM-dd");
                inboundFlights.Add(GetBasketFlight(offerResponse, inboundFlightSegments, orderResponseMS, airportData, airlineData, cabinData, airCraftData));
            }

            var sortedAssociations = offerResponse.DataList.OriginDestinationAssociations
              .OrderBy(association => offerResponse.DataList.FlightSegments
                  .Where(segment => segment.OdRefId == association.OdLabel)
                  .Min(segment => segment.ScheduledDepartureTime.LocalDateTime));


            foreach (var association in sortedAssociations)
            {
                var sliceSegments = offerResponse.DataList.FlightSegments?.Where(segment => segment.OdRefId == association.OdLabel);
                if (sliceSegments != null && sliceSegments.Any())
                {
                    slices.Add(GetBasketFlight(offerResponse, sliceSegments, orderResponseMS, airportData, airlineData, cabinData, airCraftData));
                }
            }
            var seatPrice = Convert.ToDouble(seatMaps?.Sum(seat => seat.Price.Amount));
            price += seatPrice;

            var quotePrice = basket.IsInsuranceAdded == true ? basket.InsuranceQuotes?.FirstOrDefault()?.Products.FirstOrDefault()?.TotalPrice ?? 0 : 0;
            price += quotePrice;

            var selectedPayWithPointsOptionDetails = basket.PaymentSessions?.FirstOrDefault()?.SelectedPayWithPointsOption.IsNullOrDefault() == false ? new PointsEquivalentCurrency()
            {
                Amount = Convert.ToDouble(basket.PaymentSessions?.FirstOrDefault()?.SelectedPayWithPointsOption.Amount.MonetaryAmount),
                CurrencyCode = basket.PaymentSessions?.FirstOrDefault()?.SelectedPayWithPointsOption.Amount.BaseCurrency,
                LoyaltyPoints = basket.PaymentSessions?.FirstOrDefault()?.SelectedPayWithPointsOption.LoyaltyPoints.TrimStart('0')
            } : null;


            return new BasketSummary
            {
                OutboundFlights = outboundFlights,
                InboundFlights = inboundFlights,
                Slices = slices,
                DepartureDate = departureDate,
                ReturnDate = returnDate,
                BookingType = Constants.Application.Flight,
                InsurancePrice = quotePrice,
                MinDeposit = price,
                TotalPriceWithoutDiscount = price,
                TotalPrice = price,
                PriceCurrency = offerResponse.Offers.First().SalePrice.Where(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).First().Amount.BaseCurrency,
                PricePerPerson = Math.Round(price / peoples.Count(x => x.Type != Constants.Application.InfantPtc), 2),
                PaymentAmount = price - (selectedPayWithPointsOptionDetails?.Amount ?? 0),
                PartyComposition = new()
                {
                    Adults = peoples.Count(x => x.Type == Constants.Application.AdultPtc),
                    Children = peoples.Count(x => x.Type == Constants.Application.ChildPtc),
                    Infants = peoples.Count(x => x.Type == Constants.Application.InfantPtc),
                    Teens = peoples.Count(x => x.Type == Constants.Application.TeenPtc),
                    Total = peoples.Count()
                },
                TermsAndConditions = File.ReadAllText($"Content/TermsAndConditions.txt"),
                Includes = new() { Flight = true },
                Tracking = new() { },
                Tours = Array.Empty<string>(),
                Cruises = Array.Empty<string>(),
                Cars = Array.Empty<string>(),
                Extras = Array.Empty<string>(),
                Destination = new(),
                TotalSeatPrice = seatPrice,
                FlightPrice = Convert.ToDouble(offerResponse.Offers.First().SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount),
                IsAwardSearch = basket.FlightsearchRequest.isAwardSearch,
                MembershipPoints = offerResponse.IsAwardSearchResult ? offerResponse.Offers.First().SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.PointsEnum).PointsAmount.PointsCount : 0,
                HasSufficientPoints = voucherRedemptionEnabled
                    ? basket.HasSufficientPoints ?? GetSufficientPointsFlag(offerResponse, customerProfile, basket.PointsToPay)
                    : GetSufficientPointsFlag(offerResponse, customerProfile, null),
                VoucherPoints = voucherRedemptionEnabled ? basket.VoucherPoints : null,
                PointsToPay = voucherRedemptionEnabled ? basket.PointsToPay : null,
                VoucherApplied = voucherRedemptionEnabled && basket.VoucherApplied,
                VoucherDetails = voucherRedemptionEnabled ? basket.VoucherDetails : null,
                VoucherRedemptionSelection = voucherRedemptionEnabled ? basket.VoucherRedemptionSelection : null,
                SummaryLink = basket.SummaryLink,
                SelectedPayWithPointsOption = selectedPayWithPointsOptionDetails
            };
        }

        private static bool? GetSufficientPointsFlag(OfferResponse offerResponse, BasketLoyaltyMemberDetails customerProfile, string? pointsToPay)
        {
            if (!offerResponse.IsAwardSearchResult)
            {
                return null;
            }

            var requiredPoints = long.TryParse(pointsToPay, out var voucherPointsToPay)
                ? voucherPointsToPay
                : offerResponse.Offers.First().SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.PointsEnum).PointsAmount.PointsCount;

            if (customerProfile is null)
            {
                return false;
            }

            return requiredPoints == 0 ||
                (long.TryParse(customerProfile.MembershipDetails?.FQTPoints, out var availablePoints) &&
                 availablePoints >= requiredPoints);
        }

        private static BasketFlight GetBasketFlight(OfferResponse offerResponse, IEnumerable<Segment> segments, OrderResponseMS orderResponseMS, IEnumerable<AirportRefData> airportData, IEnumerable<AirlineRefData> airlineData, IEnumerable<CabinRefData> cabinData, IEnumerable<AircraftRefData> airCraftData = null)
        {
            IList<BasketSectorDetails> Sectors = new List<BasketSectorDetails>();
            BasketFlight flight = new BasketFlight();
            BasketSectorDetails singleSector = new BasketSectorDetails();

            flight.Airline = segments.First().OperatingCarrier;
            flight.AirlineName = airlineData?.FirstOrDefault(airline => airline.CarrierCode == segments.First().OperatingCarrier)?.CarrierName;
            flight.FlightNumber = segments.First().MarketingCarrier != null ? $"{segments.First().MarketingCarrier}{segments.First().MarketingFlightNumber}" : segments.First().MarketingFlightNumber;
            flight.Arriving = new BasketAirportDetails { Iata = segments.Last().DestinationAirportCode, Name = airportData?.FirstOrDefault(airport => airport.AirportCode == segments.Last().DestinationAirportCode)?.AirportName };
            flight.Departing = new BasketAirportDetails { Iata = segments.First().OriginAirportCode, Name = airportData?.FirstOrDefault(airport => airport.AirportCode == segments.First().OriginAirportCode)?.AirportName };
            flight.DepartingAt = Common.Helper.DateTimeHelper.ParseLocalDateTime(segments.First().ScheduledDepartureTime.LocalDateTime).Value;
            flight.ArrivingAt = Common.Helper.DateTimeHelper.ParseLocalDateTime(segments.Last().ScheduledArrivalTime.LocalDateTime).Value;

            flight.Sectors = segments.Select(segment => new BasketSectorDetails()
            {
                Airline = segment.MarketingCarrier,
                CabinClass = GetCabinName(offerResponse, cabinData, segment),
                Pnr = orderResponseMS?.BookingReference?.RLOC,
                AirlineName = airlineData?.FirstOrDefault(airline => airline.CarrierCode == segment.MarketingCarrier)?.CarrierName,
                OperatingAirline = segment.OperatingCarrier,
                OperatingAirlineName = airlineData?.FirstOrDefault(airline => airline.CarrierCode == segment.OperatingCarrier)?.CarrierName,
                Leg = new BasketSectorLeg
                {
                    Airline = segment.Legs?.FirstOrDefault(leg => leg.IsDominantLeg == true)?.MarketingCarrier ?? segment.MarketingCarrier,
                    AirlineName = airlineData?.FirstOrDefault(airline => airline.CarrierCode == (segment.Legs?.FirstOrDefault(leg => leg.IsDominantLeg == true)?.MarketingCarrier ?? segment.MarketingCarrier))?.CarrierName,
                    OperatingAirline = segment.Legs?.FirstOrDefault(leg => leg.IsDominantLeg == true)?.OperatingCarrier ?? segment.OperatingCarrier,
                    OperatingAirlineName = airlineData?.FirstOrDefault(airline => airline.CarrierCode == (segment.Legs?.FirstOrDefault(leg => leg.IsDominantLeg == true)?.OperatingCarrier ?? segment.OperatingCarrier))?.CarrierName,
                },
                FlightNumber = segment.MarketingCarrier != null ? $"{segments.First().MarketingCarrier}{segment.MarketingFlightNumber}" : segment.MarketingFlightNumber,
                SectorId = segment.Legs.First().LegId,
                Arriving = new BasketAirportDetails { Iata = segment.DestinationAirportCode, Name = airportData?.FirstOrDefault(airport => airport.AirportCode == segment.DestinationAirportCode)?.AirportName },
                Departing = new BasketAirportDetails { Iata = segment.OriginAirportCode, Name = airportData?.FirstOrDefault(airport => airport.AirportCode == segment.OriginAirportCode)?.AirportName },
                ArrivingAt = Common.Helper.DateTimeHelper.ParseLocalDateTime(segment.ScheduledArrivalTime.LocalDateTime).Value,
                DepartingAt = Common.Helper.DateTimeHelper.ParseLocalDateTime(segment.ScheduledDepartureTime.LocalDateTime).Value,
                AirCraftDetail = airCraftData?.FirstOrDefault(a => a.IataCode == segment.AirCraft)?.FullName,
                Duration = Common.Helper.DateTimeHelper.ConvertColonDurationToIso(segment.FlightDuration)
            });

            flight.CabinClass = flight.Sectors.First().CabinClass;
            return flight;

        }
        private static string GetCabinName(OfferResponse offerResponse, IEnumerable<CabinRefData> cabinData, Segment segment)
        {
            var flightSegmentIndex = offerResponse.Offers.First().OfferItems.First().Services.First(x => x.FlightSegmentsRefIds.Contains(segment.SegmentId)).FlightSegmentsRefIds.ToList().FindIndex(x => x == segment.SegmentId);

            var singlefaresegment = offerResponse.Offers.First().OfferItems.First().Services.First(x => x.FlightSegmentsRefIds.Contains(segment.SegmentId)).FareDetails.FareSegments.ToArray()[flightSegmentIndex];

            return cabinData?.FirstOrDefault(cabin => cabin.BrandID == singlefaresegment.CabinName)?.BrandName;
        }
    }
}
