using VAA.EAPI.GraphQL.Domain.Loyalty.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    internal static class VSRetroClaimMapper
    {
        internal static CabinClassDetailsRequest MapToCabinClassRequest(VSRetroFlownDetail flight)
        {
            return new CabinClassDetailsRequest
            {
                OperatedAirlineCode = flight.OperatingAirlineCode,
                OperatedBookingClass = flight.FlownUpgradedClass ?? flight.OriginalBookedClass,
                MarketingAirlineCode = flight.MarketingAirlineCode,
                MarketingBookingClass = flight.VAMarketingBookingClass,
                OriginAirportCode = flight.DepartureAirport,
                DestinationAirportCode = flight.ArrivalAirport,
                FlightActivityDate = flight.DepartureDate.ToString(Common.Constants.Application.ISODateFormat)
            };
        }

        public static AirlineEarnedPointsRequest MapToPointsRequest(
            VSRetroClaimRequest request,
            CustomerDTO customer,
            AirportDistanceAndCountries route)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (customer is null) throw new ArgumentNullException(nameof(customer));

            return new()
            {
                FlightSegments = new List<FlightSegments>()
                {
                    new(){
                    OriginAirportCode = request.DepartureAirport,
                    DestinationAirportCode = request.ArrivalAirport,
                    OriginCountryCode = route.OriginCountryCode,
                    DestinationCountryCode = route.DestinationCountryCode,
                    SegmentDistance = route.DistanceInMiles.ToString(),
                    OperatingCarrier = request.OperatingAirlineCode,
                    MarketingCarrier = request.MarketingAirlineCode,
                    OperatingFlightNumber = request.OperatingFlightNumber,
                    MarketingFlightNumber = request.MarketingFlightNumber,
                    ScheduledDepartureTime = request.DepartureDate == default ? string.Empty : request.DepartureDate.ToString(Common.Constants.Application.ISODateFormat),
                    CabinClass = request.FlownCabinClass ?? string.Empty,
                    MarketingBookingClass = request.VAMarketingBookingClass,
                    OperatingBookingClass = request.OriginalBookedClass ?? string.Empty,
                    FlownClass = !string.IsNullOrEmpty(request.FlownUpgradedClass)
                    ? request.FlownUpgradedClass
                    : request.OriginalBookedClass ?? string.Empty,
                    SalesLocation = request.SalesLocation ?? string.Empty,
                    AgentIataNumber = request.IataAgentNumber ?? string.Empty,
                    FareBasisCode = request.FareBasis ?? string.Empty,
                    PassengerType = request.PassengerType ?? string.Empty ,
                    TicketDesignator = request.TicketDesignator ?? string.Empty,
                    DealCode = request.VADealCode ?? string.Empty,
                    }
                },
                CustomerDetails = new List<CustomerDetails>()
                {
                    new(){
                    MemberNumber = request.MemberNumber,
                    TierCode = customer.Customer.MemberTier,
                    MemberType = customer.Customer.MemberAccountType,
                    MemberStatus = customer.Customer.Status,
                    }
                }
            };
        }

        internal static TransactionRequest MapToTransactionRequest(
            VSRetroClaimRequest request,
            CustomerDTO customer,
            SegmentResponses points,
            AirportDistanceAndCountries route,
            PartnerProfileDetails partner,
            ProductDetails product)
        {
            var bookingClass = request.FlownUpgradedClass ?? request.OriginalBookedClass;
            var segment = new SegmentDetails
            {
                BookingClass = bookingClass,
                DepartureDate = request.DepartureDate,
                DestinationAirportcode = request.ArrivalAirport,
                MarketingCarrierCode = request.MarketingAirlineCode,
                MarketingFlightNumber = request.MarketingFlightNumber,
                OperatingFlightNumber = request.OperatingFlightNumber,
                OriginAirportcode = request.DepartureAirport,
                PartnerCode = partner.PartnerCode,
                SegmentMiles = route?.DistanceInMiles,
                TicketDesignator = request.TicketDesignator,
                FareBasisCode = request.FareBasis,
                CreatedBy = request.InitiatedBy,
                CreatedOn = DateTime.UtcNow,
                LastUpdatedBy = request.InitiatedBy,
                LastUpdatedOn = DateTime.UtcNow
            };

            return new TransactionRequest
            {
                ProductName = product.ProductName,
                PartnerName = partner.Name,
                TransactionDocument = new Transaction
                {
                    TransactionId = Utility.GetId(),
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = request.InitiatedBy,
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedBy = request.InitiatedBy,
                    CustomerId = customer.Customer.CustomerId,
                    Comments = points.ProcessingComments,
                    ActivityNumber = request.OperatingFlightNumber,
                    MemberNumber = customer.Customer.LoyaltyNumber,
                    TransactionValue = points.PointsAward,
                    PartnerId = partner.RowId,
                    ProductId = product.RowId,
                    TransactionDate = DateTime.UtcNow,
                    ActivityDate = request.DepartureDate,
                    StatusCode = Common.Constants.Application.TxnProcessingStatus,
                    TransactionSubType = Common.Constants.Application.TxnSubType,
                    TransactionType = Common.Constants.Application.AccrualTransactionType,
                    TierPointsToCredit = points.TierPointsAward,
                    TransactionChannel = Constants.Application.PATransactionChannel,
                    FlightDetails = new FlightDetails
                    {
                        BookingDate = request.TicketIssueDate,
                        BookingClass = bookingClass,
                        CabinClass = request.FlownCabinClass,
                        TravelClass = request.FlownCabinClass,
                        DestinationAirport = request.ArrivalAirport,
                        OriginAirportcode = request.DepartureAirport,
                        ExternalReservationNumber = request.RecordLocatorOperatingCarrier,
                        GDSRecordLocator = request.RecordLocatorOperatingCarrier,
                        Ticketnumber = request.TicketNumber,
                        OperatingFlightNumber = request.OperatingFlightNumber,
                        MarketingFlightNumber = request.MarketingFlightNumber,
                        MarketingPartnerCode = request.MarketingAirlineCode,
                        MarketingBookingClass = request.VAMarketingBookingClass,
                        DepartureDate = request.DepartureDate,
                        FareBasis = request.FareBasis,
                        FareBasisCode = request.FareBasis,
                        Distance = route?.DistanceInMiles,
                        RetroFlag = Common.Constants.Application.RetroFlag,
                        RetroOverrideFlag = request.OverrideFlag,
                        RetroOverrideReason = request.OverrideReason,
                        VAPostingTypeCode = Constants.Application.PostingTypeCode,
                        MIFlag = "Y",
                        OperatingAirlineAuthNumber = request.OperatingAirlineAuthNum,
                        PNRName = $"{request.LastName}/{request.FirstName}",
                        Quantity = "1",
                        NumberOfSegments = "1",
                        PassengerDetails = new[]
                        {
                            new PassengerDetails
                            {
                                PassengerName = $"{request.FirstName} {request.LastName}",
                                PhysicalTicketNumber = request.TicketNumber,
                                ValueToCredit = points.PointsAward,
                                NumberOfSegments = "1",
                                Status = customer.Customer.Status,
                                CreatedOn = DateTime.UtcNow,
                                CreatedBy = request.InitiatedBy,
                                LastUpdatedOn = DateTime.UtcNow,
                                LastUpdatedBy = request.InitiatedBy,
                                SegmentDetails = new[] { segment }
                            }
                        }
                    },
                    CreditDetails = MapCredits(points, request.InitiatedBy)
                }
            };
        }

        internal static VSClaimDetails MapFlight(VSRetroClaimRequest request, SegmentResponses points = null, bool accepted = false, string reason = null)
        {
            return new VSClaimDetails
            {
                Id = request.Id.ToString(),
                ActivityDate = request.DepartureDate,
                FlightNumber = request.OperatingFlightNumber,
                OriginationAirport = request.DepartureAirport,
                DestinationAirport = request.ArrivalAirport,
                RevenueClass = request.FlownUpgradedClass ?? request.OriginalBookedClass,
                PointsEarned = ToInt(points?.PointsAward),
                TierPointsEarned = ToInt(points?.TierPointsAward),
                IataAgentNumber = request.IataAgentNumber,
                SalesLocation = request.SalesLocation,
                RecordLocator = request.RecordLocatorOperatingCarrier,
                VADealCode = request.VADealCode,
                Accepted = accepted,
                Reason = reason
            };
        }

        internal static VSClaimDetails MapRequest(VSRetroClaimRequest request, string reason)
        {
            return new VSClaimDetails
            {
                Id = request.Id.ToString(),
                FlightNumber = request.OperatingFlightNumber,
                SalesLocation = request.SalesLocation,
                RecordLocator = request.RecordLocatorOperatingCarrier,
                VADealCode = request.VADealCode,
                Accepted = false,
                Reason = reason
            };
        }

        private static IEnumerable<CreditDetails> MapCredits(SegmentResponses points, string createdBy)
        {
            var credits = new List<CreditDetails>();
            AddCredit(credits, points.BasePoints, Common.Constants.Application.MilesType, Common.Constants.Application.BaseMilesType, Common.Constants.Application.MilesQualifyingFlag, createdBy);
            AddCredit(credits, points.CabinBonusPoints, Common.Constants.Application.MilesType, Common.Constants.Application.CabinBonusType, Common.Constants.Application.MilesQualifyingFlag, createdBy);
            AddCredit(credits, points.TierBonusPoints, Common.Constants.Application.MilesType, Common.Constants.Application.TierBonusType, Common.Constants.Application.MilesQualifyingFlag, createdBy);
            AddCredit(credits, points.TierPointsAward, Common.Constants.Application.TierPointsType, string.Empty, Common.Constants.Application.MilesQualifyingFlagForTierPoints, createdBy);
            return credits;
        }

        private static void AddCredit(List<CreditDetails> credits, string amount, string pointType, string pointSubType, string qualifyingFlag, string createdBy)
        {
            if (ToInt(amount) <= 0)
            {
                return;
            }

            credits.Add(new CreditDetails
            {
                CreditItemId = Utility.GetId(),
                CreditValue = amount,
                PointType = pointType,
                PointSubType = pointSubType,
                QualifyingFlag = qualifyingFlag,
                CreatedBy = createdBy,
                CreatedOn = DateTime.UtcNow,
                LastUpdatedBy = createdBy,
                LastUpdatedOn = DateTime.UtcNow
            });
        }

        private static int ToInt(string value) => int.TryParse(value, out var points) ? points : 0;
    }
}
