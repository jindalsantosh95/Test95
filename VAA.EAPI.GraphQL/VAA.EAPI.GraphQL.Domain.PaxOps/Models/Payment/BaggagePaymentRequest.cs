using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Payment
{
    public class BaggagePaymentRequest
    {
        public BaggagePaymentPos Pos { get; set; }
        public PaymentInfo PaymentAmt { get; set; }
        public LoyaltyMember LoyaltyMemberDetails { get; set; }
        public IEnumerable<CustomerInfo> CustomerDetails { get; set; }
        public AdditionalConfigParams AdditionalConfigParams { get; set; }
        public IEnumerable<Bags>? BagsDetails { get; set; }
        public IEnumerable<Segment> FlightSegments { get; set; }
        public string TripType { get; set; }
        public bool IsBagsPayment { get; set; }
        public string? RecordLocator { get; set; }

        public static explicit operator BaggagePaymentRequest((PaymentCheckRequest request, Basket.Basket basketResponse, string currencyNumber, string ipAddress, decimal? totalPendingAmount) source)
        {
            var purchaseBagPassengers = source.basketResponse.CheckedBaggageDetails.PassengerBaggage.Where(pb => pb.TotalPrice > 0);

            return new BaggagePaymentRequest()
            {
                Pos = new BaggagePaymentPos
                {
                    IpAddress = source.ipAddress,
                    CurrencyNumber = source.currencyNumber,
                    CurrencyCode = source.basketResponse.CheckedBaggageDetails.PassengerBaggage.FirstOrDefault()?.CurrencyCode
                },
                PaymentAmt = new PaymentInfo
                {
                    CurrencyEquivalentPrice = new CurrencyEquivalentPrice
                    {
                        Amount = new Amount
                        {
                            MonetaryAmount = source.totalPendingAmount?.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),  //creating 2 decimal until dpc issus is fixed by DL
                            DecimalPrecisionCount = 2,   // hardcoding it until this is fixed by DL
                            Currency = source.basketResponse.CheckedBaggageDetails.PassengerBaggage.FirstOrDefault()?.CurrencyCode
                        }
                    }
                },
                LoyaltyMemberDetails = new LoyaltyMember
                {
                    MemberNumber = source.basketResponse?.BookingDetails?.Passenger?.FirstOrDefault()?.MembershipDetails?.FQTNumber,
                    MemberTierCode = source.basketResponse?.BookingDetails?.Passenger?.FirstOrDefault()?.MembershipDetails?.FQTTierCode
                },
                CustomerDetails = source.basketResponse.BookingDetails.Passenger.Select(p =>
                {
                    var customer = new CustomerInfo()
                    {
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Contacts = new Contacts
                        {
                            Countrycode = source.basketResponse?.ContactDetails?.CountryCode,
                            Email = source.basketResponse?.ContactDetails?.EmailAddress,
                            Phone = source.basketResponse?.ContactDetails?.PhoneNumber,
                        },
                        DateOfBirth = p.DateOfBirth?.ToString("yyyy-MM-dd")
                    };
                    return customer;
                }),
                AdditionalConfigParams = new AdditionalConfigParams
                {
                    SuccessRedirectUrl = source.request.SuccessRedirectUrl ?? string.Empty,
                    FailureRedirectUrl = source.request.FailureRedirectUrl ?? string.Empty,
                },
                TripType = ConfigReader.Get(Common.Constants.Environment.BaggageOciTripType),
                IsBagsPayment = true,
                RecordLocator = source.basketResponse.BookingDetails.PnrLocator,

                BagsDetails = purchaseBagPassengers.Select((pb, i) => new Bags()
                {
                    PassengerSequence = (i+1).ToString(),
                    ProductDescription = Common.Constants.Application.BaggageProductDescription,
                    ProductCurrencyCode = pb.CurrencyCode,
                    ProductCurrencyAmt = pb.TotalPrice.ToString(),
                    ProductDecimalPrecisionCount = pb.DecimalPrecisionCount.ToString(),
                    ProductCode = Common.Constants.Application.BaggageProductCode,
                    ProductCategory = Common.Constants.Application.BaggageProductCategory,
                    ProductItem = Common.Constants.Application.BaggageProductItem,
                }),
                FlightSegments = source.basketResponse.BookingDetails.Slices.SelectMany(s => s.Segments.Select(seg => new Segment
                {
                    MarketingFlightNumber = seg?.MarketingCarrier?.FlightNumber,
                    MarketingCarrier = seg?.MarketingCarrier?.CarrierCode,
                    OperatingCarrier = seg?.OperatingCarrier?.CarrierCode,
                    ClassOfService = source.basketResponse?.BookingDetails?.Passenger?.FirstOrDefault()?.BookingClass?.Code,
                    Legs = seg.Legs.Select(l => new Leg
                    {
                        OriginAirportCode = l.Origin.Code,
                        DestinationAirportCode = l.Destination.Code,
                        ScheduledDepartureTime = new UtcAndLocalDateTime
                        {
                            LocalDateTime = l.ScheduledDepartureLocalDateTime?.ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        },
                        ScheduledArrivalTime = new UtcAndLocalDateTime
                        {
                            LocalDateTime = l.ScheduledArrivalLocalDateTime?.ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        }
                    })
                }))
            };
        }
    }
}