using System.Collections.Generic;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;

namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class Summary
    {
        public object VoucherBalance { get; set; }
        public object VoucherAmount { get; set; }
        public Tracking Tracking { get; set; }
        public IEnumerable<object> Tours { get; set; }
        public IEnumerable<object> Cruises { get; set; }
        public bool SeatsNotBooked { get; set; }
        public object VoucherExpiry { get; set; }
        public PartyComposition PartyComposition { get; set; }
        public int MembershipMiles { get; set; }
        public string BookingType { get; set; }
        public object Pennies { get; set; }
        public float MinDeposit { get; set; }
        public object VoucherRemaining { get; set; }
        public bool ResortFees { get; set; }
        public object IneligibleMember { get; set; }
        public object PromotionalDiscount { get; set; }
        public Includes Includes { get; set; }
        public object InactivePromotionalDiscount { get; set; }
        public int MembershipTierMiles { get; set; }
        public bool DirectDebit { get; set; }
        public double TotalPriceWithoutDiscount { get; set; }
        public IEnumerable<object> Cars { get; set; }
        public object Name { get; set; }
        public double PricePerPerson { get; set; }
        public int MinimumMembershipMiles { get; set; }
        public double TotalPrice { get; set; }
        public string PriceCurrency { get; set; }
        public string BalanceDueDate { get; set; }
        public string LocationDescription { get; set; }
        public OriginNavigation OriginNavigation { get; set; }
        public Destination Destination { get; set; }
        public object MembershipDiscount { get; set; }
        public IEnumerable<object> Extras { get; set; }
        public float PaymentAmount { get; set; }
        public string TermsAndConditions { get; set; }
        public int Duration { get; set; }
        public string ReturnDate { get; set; }
        public object OnlineDiscount { get; set; }
        public int MembershipMilesMultiplier { get; set; }
        public object Transfers { get; set; }
        public object LocationId { get; set; }
        public object MilesExchangeRate { get; set; }
        public string UpSellShown { get; set; }
        public object MembershipNumberType { get; set; }
        public object DiscountType { get; set; }
        public string DepartureDate { get; set; }
        public IEnumerable<Flight> OutboundFlights { get; set; }
        public IEnumerable<object> LastChanceUpSellOptions { get; set; }
        public IEnumerable<Flight> InboundFlights { get; set; }
        public IEnumerable<Flight> Slices { get; set; }        
        public object MilesAvailable { get; set; }
        public object LocationName { get; set; }
        public object BalanceDue { get; set; }
        public IEnumerable<object> LastChanceAddons { get; set; }
        public object MilesBalance { get; set; }
        public object MembershipNumber { get; set; }
        public IEnumerable<object> Hotels { get; set; }
        public object UpSell { get; set; }
        public object MilesIncrements { get; set; }
        public object PromotionalCodeValue { get; set; }
        public double TotalSeatPrice { get; set; }
        public bool IsAwardSearch { get; set; }
        public double FlightPrice { get; set; }
        public int MembershipPoints { get; set; }
        public string? VoucherPoints { get; set; }
        public string? PointsToPay { get; set; }
        public bool VoucherApplied { get; set; }
        public VoucherRedemptionDetails? VoucherDetails { get; set; }
        public VoucherRedemptionSelection? VoucherRedemptionSelection { get; set; }
        public IEnumerable<FlightPassengerSeatMapSummary> FlightPassengerSeatMap { get; set; }
        public bool? HasSufficientPoints { get; set; }
        public string SummaryLink { get; set; }
        public PointsEquivalentCurrency SelectedPayWithPointsOption { get; set; }
        public double? InsurancePrice { get; set; }
        public IEnumerable<FlightPassengerBaggage> FlightPassengerBaggage { get; set; }

    }
}
