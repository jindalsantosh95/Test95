using VAA.EAPI.GraphQL.Domain.Book.Constants;
using VAA.EAPI.GraphQL.Domain.Book.Models.Insurance;
using VAA.EAPI.GraphQL.Domain.Book.Models.Payment;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Basket
{
    public class Basket
    {
        public Guid BasketId { get; set; }
        public IEnumerable<BasketPerson> People { get; set; }
        public BasketContactInfo ContactDetails { get; set; }
        public BasketTermsAndConditions TermsAndConditions { get; set; }
        public string Validation { get; set; }
        public BasketSummary Summary { get; set; }
        public string ResourceType { get; set; }
        public IEnumerable<string> OfferIds { get; set; }
        public FlightsearchRequest FlightsearchRequest { get; set; }
        public IEnumerable<PaymentSession> PaymentSessions { get; set; }
        public BasketStatus Status { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }
        public BasketLoyaltyMemberDetails CustomerProfile { get; set; }
        public string FlightSummary { get; set; }
        public string SummaryLink { get; set; }
        public FCEarnPoints EarnPoints { get; set; }
        public FcPointsPerTier EarnPointPerTier { get; set; }
        public IEnumerable<PtcWisePointsEquivalentCurrencyDetails> PtcWisePointsEquivalentCurrencyDetails { get; set; }
        public IEnumerable<QuoteResponse> InsuranceQuotes { get; set; }
        public bool? IsInsuranceAdded { get; set; }
        public PointOfSale? PointOfSale { get; set; }
        public bool IsCoBrandEligible { get; set; }
        public CoBrandStatusEnum CoBrandStatus { get; set; }
        public bool? FcAutoMatched { get; set; }
        public string MetaPartnerId { get; set; }
        public bool? HasSufficientPoints { get; set; }
        public string? VoucherPoints { get; set; }
        public string? PointsToPay { get; set; }
        public bool VoucherApplied { get; set; }
        public Models.FlightSummary.VoucherRedemptionDetails? VoucherDetails { get; set; }
        public Models.FlightSummary.VoucherRedemptionSelection? VoucherRedemptionSelection { get; set; }
    }
}
