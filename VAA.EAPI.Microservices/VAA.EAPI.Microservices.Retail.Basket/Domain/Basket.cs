using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Insurance;
using VAA.EAPI.Microservices.Retail.Basket.Domain.PointsCurrencyConverter;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Seat;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Trips;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public sealed class Basket
    {
        public static explicit operator Basket(CreateBasketRequest source)
        {
            if (source is null) return new Basket() { BasketId = Guid.NewGuid() };

            var basket = new Basket()
            {
                BasketId = source.Basket.BasketId.Equals(Guid.Empty) ? Guid.NewGuid() : source.Basket.BasketId,
                People = source.Basket.People ?? Enumerable.Empty<Person>(),
                ContactDetails = source.Basket.ContactDetails ?? new(),
                AddressDetails = source.Basket.AddressDetails ?? new(),
                TermsAndConditions = source.Basket.TermsAndConditions ?? new(),
                OfferIds = source.Basket.OfferIds,
                BookingJourneyOfferDetails = source.Basket.BookingJourneyOfferDetails,
                FlightsearchRequest = (source.Basket.FlightsearchRequest?.SearchOriginDestinations.Any() ?? false) ? source.Basket.FlightsearchRequest : new(),
                CustomerProfile = source.Basket.CustomerProfile,
                EarnPoints = source.Basket.EarnPoints ?? new(),
                EarnPointPerTier = source.Basket.EarnPointPerTier ?? new(),
                InsuranceQuotes = source.Basket.InsuranceQuotes ?? Enumerable.Empty<InsuranceQuote>(),
                FlightSummary = source.Basket.FlightSummary,
                SummaryLink = source.Basket.SummaryLink,
                VoucherDetails = source.Basket.VoucherDetails,
                HasSufficientPoints = source.Basket.HasSufficientPoints,
                VoucherPoints = source.Basket.VoucherPoints,
                PointsToPay = source.Basket.PointsToPay,
                VoucherApplied = source.Basket.VoucherApplied,
                VoucherRedemptionSelection = source.Basket.VoucherRedemptionSelection,
                MetaPartnerId = source.Basket.MetaPartnerId,
                BookingDetails = source.Basket.BookingDetails ?? new(),
                FlightPassengerSeatMap = source.Basket.FlightPassengerSeatMap ?? Enumerable.Empty<FlightPassengerSeatMap>(),
                CheckedBaggageDetails = source.Basket.CheckedBaggageDetails
            };

            int index = 0;

            foreach (var item in basket.People)
            {
                item.Validation = Constants.Application.InvalidStatus;
                item.Id = index.ToString();

                index++;
            }

            return basket;
        }


        public Guid BasketId { get; set; }
        public IEnumerable<Person> People { get; set; }
        public Address AddressDetails { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public TermsAndConditions TermsAndConditions { get; set; }
        public IEnumerable<string> OfferIds { get; set; }
        public IEnumerable<BookingJourneyOfferDetails> BookingJourneyOfferDetails { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string PaymentLink { get; set; }
        public string ResourceType { get; set; }
        public FlightsearchRequest FlightsearchRequest { get; set; }
        public IEnumerable<PaymentSession> PaymentSessions { get; set; }
        public BasketStatus Status { get; set; }
        public IEnumerable<FlightPassengerSeatMap> FlightPassengerSeatMap { get; set; }
        public LoyaltyMemberDetails CustomerProfile { get; set; }
        public string FlightSummary { get; set; }
        public string SummaryLink { get; set; }
        public VoucherDetails VoucherDetails { get; set; }
        public bool? HasSufficientPoints { get; set; }
        public string? VoucherPoints { get; set; }
        public string? PointsToPay { get; set; }
        public bool VoucherApplied { get; set; }
        public VoucherRedemptionSelection? VoucherRedemptionSelection { get; set; }
        public FCEarnPoints.EarnPoints EarnPoints { get; set; }
        public FcPointsPerTier EarnPointPerTier { get; set; }
        public IEnumerable<PtcWisePointsEquivalentCurrencyDetails> PtcWisePointsEquivalentCurrencyDetails { get; set; }
        public IEnumerable<InsuranceQuote>? InsuranceQuotes { get; set; }
        public bool? IsInsuranceAdded { get; set; }
        public string MetaPartnerId { get; set; }
        public CoBrandStatusEnum CoBrandStatus { get; set; }
        public Booking BookingDetails { get; set; }
        public bool? FcAutoMatched { get; set; }
        public CheckedBaggage CheckedBaggageDetails { get; set; }
    }
}
