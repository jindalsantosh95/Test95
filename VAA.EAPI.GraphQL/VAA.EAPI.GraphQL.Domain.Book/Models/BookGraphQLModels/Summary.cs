using HotChocolate;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Common;
using VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels
{
    [GraphQLName("bookSummary")]
    public class Summary
    {
        public string VoucherBalance { get; set; }
        public string VoucherAmount { get; set; }
        public Tracking Tracking { get; set; }
        public IEnumerable<string> Tours { get; set; }
        public IEnumerable<string> Cruises { get; set; }
        public bool SeatsNotBooked { get; set; }
        public string VoucherExpiry { get; set; }
        public GQLPartyComposition PartyComposition { get; set; }
        public int MembershipMiles { get; set; }
        public string BookingType { get; set; }
        public string Pennies { get; set; }
        public double MinDeposit { get; set; }
        public string VoucherRemaining { get; set; }
        public bool ResortFees { get; set; }
        public string IneligibleMember { get; set; }
        public string PromotionalDiscount { get; set; }
        public Includes Includes { get; set; }
        public string InactivePromotionalDiscount { get; set; }
        public int MembershipTierMiles { get; set; }
        public bool DirectDebit { get; set; }
        public double TotalPriceWithoutDiscount { get; set; }
        public IEnumerable<string> Cars { get; set; }
        public string Name { get; set; }
        public double PricePerPerson { get; set; }
        public int MinimumMembershipMiles { get; set; }
        public double TotalPrice { get; set; }
        public string PriceCurrency { get; set; }
        public string BalanceDueDate { get; set; }
        public string LocationDescription { get; set; }
        public string MembershipDiscount { get; set; }
        public IEnumerable<string> Extras { get; set; }
        public double PaymentAmount { get; set; }
        public string TermsAndConditions { get; set; }
        public int Duration { get; set; }
        public string ReturnDate { get; set; }
        public string OnlineDiscount { get; set; }
        public int MembershipMilesMultiplier { get; set; }
        public string Transfers { get; set; }
        public string LocationId { get; set; }
        public string MilesExchangeRate { get; set; }
        public string UpSellShown { get; set; }
        public string MembershipNumberType { get; set; }
        public string DiscountType { get; set; }
        public string DepartureDate { get; set; }
        public IEnumerable<Flight> OutboundFlights { get; set; }
        public IEnumerable<string> LastChanceUpSellOptions { get; set; }
        public IEnumerable<Flight> InboundFlights { get; set; }
        public IEnumerable<Flight> Slices { get; set; }
        public string MilesAvailable { get; set; }
        public string LocationName { get; set; }
        public double BalanceDue { get; set; }
        public IEnumerable<string> LastChanceAddons { get; set; }
        public string MilesBalance { get; set; }
        public string MembershipNumber { get; set; }
        public IEnumerable<string> Hotels { get; set; }
        public string UpSell { get; set; }
        public string MilesIncrements { get; set; }
        public string PromotionalCodeValue { get; set; }
        public double TotalSeatPrice { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }
        public bool IsAwardSearch { get; set; }
        public double FlightPrice { get; set; }
        public double InsurancePrice { get; set; }
        public int MembershipPoints { get; set; }
        public PointsEquivalentCurrency SelectedPayWithPointsOption { get; set; }
        public IEnumerable<FlightPassengerBaggage> FlightPassengerBaggage { get; set; }
        public bool VoucherApplied { get; set; }
        public bool? HasSufficientPoints { get; set; }
        public string VoucherPoints { get; set; }
        public string PointsToPay { get; set; }
        public VoucherRedemptionDetails VoucherDetails { get; set; }
        public VoucherRedemptionSelection VoucherRedemptionSelection { get; set; }

        public static explicit operator Summary(BasketSummary source)
        {
            return Json.Deserialize<Summary>(Json.Serialize(source));
        }
    }
}
