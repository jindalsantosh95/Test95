using Microsoft.Extensions.Logging;
using System.Globalization;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Search.Helpers;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Services;
using VAA.EAPI.GraphQL.Domain.Search.Interfaces;
using VAA.EAPI.GraphQL.Domain.Search.Mapper;
using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer.SummaryDeepLink;
using VAA.EAPI.GraphQL.Domain.Search.Models.Reference;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Summary = VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary.Summary;

namespace VAA.EAPI.GraphQL.Domain.Search.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class SummaryQueryBase
    {
        private readonly ILogger<SummaryQueryBFF> _logger;
        private readonly IOfferPriceServices _offerPriceService;
        private readonly IOfferRetrieveService _offerRetrieveService;
        private readonly IBasketService _basketService;
        private readonly IBrandService _brandService;
        private readonly IRefDataService<AircraftInfo> _aircraftInfoCacheService;
        private readonly IRefDataService<AirportInfo> _airportInfoCacheService;
        private readonly IRefDataService<AirlineInfo> _airlineInfoCacheService;
        private readonly IRefDataService<CabinInfo> _cabinInfoCacheService;
        private readonly IRefDataService<BaggageDetails> _baggageDetailsCacheService;
        private readonly IRefDataService<TaxCodeInfo> _taxcodeInfoCacheService;
        private readonly IRefDataService<UpgradeInfo> _upgradeInfoCacheService;
        private readonly ITokenValidatorExtension _tokenValidatorExtension;
        private readonly IVRedServices _vRedServices;
        private readonly IInRuleService _inRuleService;
        private readonly ICustomerProvider _customerProvider;
        private readonly ICustomerTransactionProvider _customerTransactionProvider;
        private readonly IVoucherRedemptionService _voucherRedemptionService;

        public SummaryQueryBase(ILogger<SummaryQueryBFF> logger, IOfferPriceServices offerPriceService, IOfferRetrieveService offerRetrieveService, IBasketService basketService, IBrandService brandService,
            IRefDataService<AircraftInfo> aircraftInfoCacheService, IRefDataService<AirportInfo> airportInfoCacheService,
            IRefDataService<AirlineInfo> airlineInfoCacheService, IRefDataService<CabinInfo> cabinInfoCacheService, IRefDataService<BaggageDetails> baggageDetailsCacheService,
            IRefDataService<TaxCodeInfo> taxcodeInfoCacheService, IRefDataService<UpgradeInfo> upgradeInfoCacheService, ITokenValidatorExtension tokenValidatorExtension,
            IVRedServices vRedServices, IInRuleService inRuleService, ICustomerProvider customerProvider,
            ICustomerTransactionProvider customerTransactionProvider, IVoucherRedemptionService voucherRedemptionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _offerPriceService = offerPriceService ?? throw new ArgumentNullException(nameof(offerPriceService));
            _offerRetrieveService = offerRetrieveService ?? throw new ArgumentNullException(nameof(offerRetrieveService));
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
            _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
            _aircraftInfoCacheService = aircraftInfoCacheService ?? throw new ArgumentNullException(nameof(aircraftInfoCacheService));
            _airportInfoCacheService = airportInfoCacheService ?? throw new ArgumentNullException(nameof(airportInfoCacheService));
            _airlineInfoCacheService = airlineInfoCacheService ?? throw new ArgumentNullException(nameof(airlineInfoCacheService));
            _cabinInfoCacheService = cabinInfoCacheService ?? throw new ArgumentNullException(nameof(cabinInfoCacheService));
            _baggageDetailsCacheService = baggageDetailsCacheService ?? throw new ArgumentNullException(nameof(_baggageDetailsCacheService));
            _taxcodeInfoCacheService = taxcodeInfoCacheService ?? throw new ArgumentNullException(nameof(taxcodeInfoCacheService));
            _upgradeInfoCacheService = upgradeInfoCacheService ?? throw new ArgumentNullException(nameof(upgradeInfoCacheService));
            _tokenValidatorExtension = tokenValidatorExtension ?? throw new ArgumentNullException(nameof(tokenValidatorExtension));
            _vRedServices = vRedServices ?? throw new ArgumentNullException(nameof(vRedServices));
            _inRuleService = inRuleService ?? throw new ArgumentNullException(nameof(inRuleService));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _customerTransactionProvider = customerTransactionProvider ?? throw new ArgumentNullException(nameof(customerTransactionProvider));
            _voucherRedemptionService = voucherRedemptionService ?? throw new ArgumentNullException(nameof(voucherRedemptionService));
        }

        protected async Task<Summary> SearchSummaryShared(Basket basketData, bool isAwardSearch, string? summaryLink, Task<FlightOfferResponse> flightSummaryResponseTask, Amount deeplinkInitialAmount, string? offerId = null)
        {

            Task<VRedResponse>? vredResponseTask = default;
            Task<CustomerDTO>? customerDetailsTask = default;
            var loyaltydetails = new BasketLoyaltyMemberDetails();
            Task<IEnumerable<VoucherDetail>>? voucherPointsTask = null;

            // Grab a bunch of metadata while we're waiting
            var token = TokenHelper.GetAuthorizationToken();
            var flyingClubNumber = await _tokenValidatorExtension.ValidateTokenExtractFCNumber(token); //FC no be to be stored in Basket and validate points in upcoming work items

            if (!string.IsNullOrEmpty(flyingClubNumber) && isAwardSearch)
            {
                vredResponseTask = _vRedServices.GetVRedPoints(flyingClubNumber);
            }

            if (!string.IsNullOrEmpty(flyingClubNumber))
            {
                customerDetailsTask = _customerProvider.GetCustomer(null, flyingClubNumber);
            }
            var baggageDetailsTask = _baggageDetailsCacheService.TryQuery();
            var aircraftInfoTask = _aircraftInfoCacheService.TryQuery();
            var airlineInfoTask = _airlineInfoCacheService.TryQuery();
            var taxCodeInfoTask = _taxcodeInfoCacheService.TryQuery();
            var cabinInfoTask = _cabinInfoCacheService.TryQuery();
            var upgradeDetailsTask = _upgradeInfoCacheService.TryQuery();

            if (!string.IsNullOrEmpty(flyingClubNumber))
            {
                loyaltydetails = new BasketLoyaltyMemberDetails()
                {
                    MembershipDetails = new() { FQTNumber = flyingClubNumber }
                };
            }

            // Can't do any more until the flightSummary is ready.
            var flightSummaryResponse = await flightSummaryResponseTask;
            var selectedOfferId = !offerId.IsNullOrWhiteSpace()
                ? offerId
                : flightSummaryResponse?.Offers?.FirstOrDefault(offer => offer.Selected)?.OfferId;

            var isVoucherRedemptionEnabled = IsVoucherRedemptionEnabled(isAwardSearch, flightSummaryResponse);
            var promoCode = BrandMsRequestMapper.GetAppliedPromoCode(flightSummaryResponse);

            var brandRQ = BrandMsRequestMapper.BrandMsReqMapper(
                flightSummaryResponse,
                await cabinInfoTask,
                basketData.People.Select(cust => cust.Type),
                promoCode);

            var brandRsTask = _brandService.GetBrandAsync(brandRQ);

            CustomerDTO? customerDetails = string.IsNullOrEmpty(flyingClubNumber) ? null : await customerDetailsTask;

            var fcEarnPoints = await CalculateFCEarnPoints(flightSummaryResponse, customerDetails, basketData);

            if (isVoucherRedemptionEnabled && !string.IsNullOrEmpty(customerDetails?.Customer?.CustomerId))
            {
                voucherPointsTask = _customerTransactionProvider.RetrieveVoucher(customerDetails.Customer.CustomerId);
            }
            var airportCodes = flightSummaryResponse!.DataList.FlightSegments.SelectMany(fs => fs.Legs).SelectMany(x => new string[] { x.OriginAirportCode, x.DestinationAirportCode })
                                                                            .Concat(flightSummaryResponse.DataList.OriginDestinationAssociations.SelectMany(x => new string[] { x.OriginDestination.OriginAirport, x.OriginDestination.DestinationAirport }))
                                                                            .Concat(basketData.FlightsearchRequest.SearchOriginDestinations.Select(x => x.Origin))
                                                                            .Concat(basketData.FlightsearchRequest.SearchOriginDestinations.Select(x => x.Destination))
                                                                            .Distinct()
                                                                            .ToArray();
            var airportDetailsTask = _airportInfoCacheService.TryQuery(airportCodes);

            Brand? brandRs;
            try
            {
                brandRs = await brandRsTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex.Message}", ex);
                brandRs = null;
            }

            var vredResponse = (vredResponseTask != null) ? await vredResponseTask : null;

            loyaltydetails.CustomerId = customerDetails?.Customer?.CustomerId;

            var summaryResponse = OfferMStoBFFSummaryMapper.Map(
                flightSummaryResponse, flightSummaryResponse.DataList.OriginDestinationAssociations.Count(), await airportDetailsTask, await aircraftInfoTask, await airlineInfoTask,
                basketData.BasketId, basketData.BookingJourneyOfferDetails, brandRs, basketData.GetPassengerCounts(),
                await cabinInfoTask, await upgradeDetailsTask, await taxCodeInfoTask, await baggageDetailsTask, vredResponse, deeplinkInitialAmount, fcEarnPoints);

            if (isVoucherRedemptionEnabled)
            {
                var voucherDetails = await RetrieveVoucherPoints(
                    voucherPointsTask ?? Task.FromResult<IEnumerable<VoucherDetail>>(Enumerable.Empty<VoucherDetail>()));
                var accountAvailableVouchers = (voucherDetails ?? Enumerable.Empty<VoucherDetail>())
                    .Where(v => v.Status.Is("Available"))
                    .OrderBy(v => v.ExpiryDate)
                    .ToArray();
                var voucherEligibility = VoucherRedemptionEligibility.GetVoucherEligibility(
                    accountAvailableVouchers,
                    basketData.FlightsearchRequest?.SearchOriginDestinations);
                var isVoucherJourneyWithinSectorLimit = IsVoucherJourneyWithinSectorLimit(summaryResponse.Flights);
                var availableVouchers = VoucherRedemptionEligibility.GetVouchersValidForOutboundTravel(
                    accountAvailableVouchers,
                    basketData.FlightsearchRequest?.SearchOriginDestinations);
                var vouchersForRedemption = isVoucherJourneyWithinSectorLimit
                    ? GetEarliestExpiringVouchers(availableVouchers)
                    : Array.Empty<VoucherDetail>();
                ApplyVoucherJourneyResponse(
                    summaryResponse,
                    voucherEligibility,
                    isVoucherJourneyWithinSectorLimit,
                    accountAvailableVouchers,
                    vouchersForRedemption);

                var voucherCount = vouchersForRedemption.Length;

                var eligiblePaxCountExcludingInfants = VoucherRedemptionEligibility.GetEligiblePassengerCountExcludingInfants(summaryResponse.FareCalculationSummary?.Fares);
                var eligibleCompanionPaxCount = Math.Min(2, eligiblePaxCountExcludingInfants / 2);
                var isDirectVirginAtlanticOneWay = IsDirectVirginAtlanticOneWay(
                    basketData.FlightsearchRequest?.SearchOriginDestinations?.Count() ?? 0,
                    summaryResponse.Flights);
                var maximumCompanionVouchersForJourney = GetMaximumCompanionVouchersForJourney(isDirectVirginAtlanticOneWay);
                var maximumUpgradeVouchersForJourney = Constants.Application.MaximumVouchersForRedemption;
                var redemptionOptions = new List<RedemptionOption>();

                if (voucherCount > 0 && eligiblePaxCountExcludingInfants > 1 && eligibleCompanionPaxCount > 0)
                {
                    var companionAwardPointsText = summaryResponse.FareCalculationSummary?.TotalAwardPoints ?? summaryResponse.Price?.AwardPoints;
                    var totalAwardPointsText = summaryResponse.Price?.AwardPoints ?? companionAwardPointsText;

                    if (long.TryParse(companionAwardPointsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var companionAwardPoints) &&
                        companionAwardPoints > 0 &&
                        long.TryParse(totalAwardPointsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var totalAwardPointsNeededToPay) &&
                        totalAwardPointsNeededToPay > 0)
                    {
                        // Only seats covered by Companion entitlement receive the
                        // benefit.  This keeps additional passengers payable with
                        // Virgin Points rather than treating every booking as a
                        // two-passenger booking.
                        var totalPointsForCompanionToPay = companionAwardPoints * eligibleCompanionPaxCount / eligiblePaxCountExcludingInfants;

                        var redemptionSummaryResponse = await _voucherRedemptionService.GetVoucherRedemptionOffersForCompanion(
                            flightSummaryResponse,
                            vouchersForRedemption,
                            eligibleCompanionPaxCount,
                            basketData.BookingJourneyOfferDetails,
                            totalPointsForCompanionToPay,
                            totalAwardPointsNeededToPay,
                            maxVouchersToUse: Math.Min(maximumCompanionVouchersForJourney, eligibleCompanionPaxCount),
                            voucherIdToUse: null);

                        if (redemptionSummaryResponse != null)
                        {
                            var bestCompanionOption = redemptionSummaryResponse.Value.RedemptionOptions
                                .FirstOrDefault(option => option.OptionId == redemptionSummaryResponse.Value.BestOption.OptionId);
                            if (bestCompanionOption != null)
                            {
                                redemptionOptions.Add(bestCompanionOption);
                            }
                        }
                    }
                }

                if (voucherCount > 0)
                {
                    var offerDetailsTask = !selectedOfferId.IsNullOrWhiteSpace()
                        ? _offerRetrieveService.OfferRetrieveAsync(new OfferSearchRequest { OfferID = selectedOfferId })
                        : null;
                    var isReturnJourney = basketData.FlightsearchRequest?.SearchOriginDestinations?.Count() == 2;
                    var returnTripOfferId = isReturnJourney
                        ? basketData.BookingJourneyOfferDetails?.FirstOrDefault()?.OfferId
                        : null;
                    var returnOfferDetailsTask = !returnTripOfferId.IsNullOrWhiteSpace()
                        ? _offerRetrieveService.OfferRetrieveAsync(new OfferSearchRequest { OfferID = returnTripOfferId })
                        : null;
                    var offerDetailsResponseTask = RetrieveOfferDetailsForVoucherUpgrade(offerDetailsTask, selectedOfferId);
                    var returnOfferDetailsResponseTask = RetrieveOfferDetailsForVoucherUpgrade(returnOfferDetailsTask, returnTripOfferId);
                    await Task.WhenAll(offerDetailsResponseTask, returnOfferDetailsResponseTask);
                    var offerDetailsResponse = await offerDetailsResponseTask;
                    var returnOfferDetailsResponse = await returnOfferDetailsResponseTask;
                    if (VoucherUpgradePointsCalculator.TryCalculateTotalUpgradePoints(
                        new[] { offerDetailsResponse, returnOfferDetailsResponse },
                        summaryResponse.Flights,
                        eligiblePaxCountExcludingInfants,
                        out var totalUpgradePoints,
                        out var totalBaselinePoints))
                    {
                        var requiredAwardPointsText = summaryResponse.Price?.AwardPoints ?? summaryResponse.FareCalculationSummary?.TotalAwardPoints;
                        if (long.TryParse(requiredAwardPointsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var totalAwardPointsNeededToPay) &&
                            totalAwardPointsNeededToPay > 0)
                        {
                            var upgradeRedemptionResponse = await _voucherRedemptionService.GetVoucherRedemptionOffersForUpgrade(
                                flightSummaryResponse,
                                vouchersForRedemption,
                                eligiblePaxCountExcludingInfants,
                                totalUpgradePoints,
                                totalAwardPointsNeededToPay,
                                totalBaselinePoints,
                                maxVouchersToUse: maximumUpgradeVouchersForJourney,
                                voucherIdToUse: null);

                            if (upgradeRedemptionResponse != null)
                            {
                                var bestUpgradeOption = upgradeRedemptionResponse.Value.RedemptionOptions
                                    .FirstOrDefault(option => option.OptionId == upgradeRedemptionResponse.Value.BestOption.OptionId);
                                if (bestUpgradeOption != null)
                                {
                                    redemptionOptions.Insert(0, bestUpgradeOption);
                                }
                            }
                        }
                    }
                }

                if (redemptionOptions.Count > 0)
                {
                    var orderedOptions = redemptionOptions.Select((option, index) =>
                    {
                        option.OptionId = index + 1;
                        return option;
                    }).ToArray();

                    summaryResponse.VoucherDetails ??= new VoucherRedemptionDetails();
                    summaryResponse.VoucherDetails.RedemptionOptions = orderedOptions;
                    summaryResponse.VoucherDetails.BestOption = _voucherRedemptionService.SelectBestRedemptionOption(orderedOptions);
                    summaryResponse.ApplyVoucherDetails(summaryResponse.VoucherDetails, vredResponse?.Current);
                }
            }

            var basketResponse = await _basketService.CreateBasketAsync(BasketRequestMapper.UpdateFromSummary(basketData, fcEarnPoints, loyaltydetails, flightSummaryResponse.Offers.Where(o => o.Selected).Select(o => o.OfferId), summaryResponse, summaryLink));
            return summaryResponse;
        }

        internal static bool IsVoucherRedemptionEnabled(bool isAwardSearch, FlightOfferResponse flightSummaryResponse)
        {
            var flightSegments = flightSummaryResponse.DataList?.FlightSegments?.ToArray();
            return isAwardSearch
                && ConfigReader.Try<bool>(Constants.Environment.EnableVoucherRedemption, false)
                && flightSegments is { Length: > 0 }
                && flightSegments.All(segment =>
                    string.Equals(segment.OperatingCarrier, "VS", StringComparison.OrdinalIgnoreCase));
        }

        internal static VoucherDetail[] GetEarliestExpiringVouchers(IEnumerable<VoucherDetail>? vouchers)
        {
            return (vouchers ?? Enumerable.Empty<VoucherDetail>())
                .OrderBy(voucher => voucher.ExpiryDate)
                .Take(Constants.Application.MaximumVouchersForRedemption)
                .ToArray();
        }

        internal static bool IsDirectVirginAtlanticOneWay(
            int originDestinationCount,
            IEnumerable<Models.BFFSummary.Flight>? flights)
        {
            if (originDestinationCount != 1)
            {
                return false;
            }

            var segments = (flights ?? Enumerable.Empty<Models.BFFSummary.Flight>())
                .SelectMany(flight => flight.Segments ?? Enumerable.Empty<Models.BFFSummary.Segment>())
                .ToArray();

            return segments.Length == 1
                && segments[0].StopCount == 0
                && segments[0].OperatingFlightNumber?.StartsWith("VS", StringComparison.OrdinalIgnoreCase) == true;
        }

        internal static bool IsVoucherJourneyWithinSectorLimit(IEnumerable<Models.BFFSummary.Flight>? flights)
        {
            var sectorCount = (flights ?? Enumerable.Empty<Models.BFFSummary.Flight>())
                .SelectMany(flight => flight.Segments ?? Enumerable.Empty<Models.BFFSummary.Segment>())
                .Sum(segment => segment.Legs?.Count() ?? 1);

            return sectorCount is > 0 and <= 2;
        }

        internal static void ApplyVoucherJourneyResponse(
            Summary summaryResponse,
            VoucherEligibility voucherEligibility,
            bool isVoucherJourneyWithinSectorLimit,
            IEnumerable<VoucherDetail> accountAvailableVouchers,
            IEnumerable<VoucherDetail> vouchersForRedemption)
        {
            summaryResponse.VoucherEligibility = isVoucherJourneyWithinSectorLimit
                ? voucherEligibility
                : new VoucherEligibility
                {
                    Eligible = false,
                    ReasonCode = Constants.Application.VoucherJourneySectorLimitReasonCode
                };

            if (!accountAvailableVouchers.Any())
            {
                return;
            }

            summaryResponse.VoucherDetails = new VoucherRedemptionDetails
            {
                AvailableVouchers = vouchersForRedemption
                    .Select(voucher => new Models.BFFSummary.AvailableVoucher
                    {
                        VoucherId = voucher.VoucherNumber,
                        Type = voucher.Category,
                        BalancePoints = long.TryParse(voucher.MaxValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var points) ? points : 0,
                        ExpiryDate = voucher.ExpiryDate
                    })
                    .ToArray()
            };
        }

        internal static int GetMaximumCompanionVouchersForJourney(bool isDirectVirginAtlanticOneWay)
        {
            return isDirectVirginAtlanticOneWay ? 1 : Constants.Application.MaximumVouchersForRedemption;
        }

        private async Task<IEnumerable<VoucherDetail>> RetrieveVoucherPoints(Task<IEnumerable<VoucherDetail>> voucherPointsTask)
        {
            try
            {
                return voucherPointsTask == null ? null : await voucherPointsTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception while retrieving voucher details from Loops - {ex.Message}", ex);
                return null;
            }
        }

        private async Task<FlightOfferResponse?> RetrieveOfferDetailsForVoucherUpgrade(
            Task<FlightOfferResponse>? offerDetailsTask,
            string? offerId)
        {
            if (offerDetailsTask is null)
            {
                return null;
            }

            try
            {
                return await offerDetailsTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Unable to retrieve Offer MS details for voucher upgrade calculation for offer {OfferId}",
                    offerId);
                return null;
            }
        }

        private async Task<EarnPoints> CalculateFCEarnPoints(FlightOfferResponse flightSummaryResponse, CustomerDTO? customerDetails, Basket basket)
        {
            var currentTier = customerDetails?.Customer?.MemberTier ?? "Red";

            var existingCalculatedTier = basket.EarnPointPerTier?.Tiers?.FirstOrDefault()?.TierType;

            var hasPassengerDetails = basket.People.Any(p => !string.IsNullOrWhiteSpace(p.FirstName) && !string.IsNullOrWhiteSpace(p.LastName));

            var shouldReuseExistingPoints = basket.EarnPoints != null && basket.EarnPointPerTier?.Tiers?.Any() == true && string.Equals(existingCalculatedTier, currentTier, StringComparison.OrdinalIgnoreCase);

            // Reuse if passenger details presnt and tier matches
            if (hasPassengerDetails && shouldReuseExistingPoints)
            {
                return new EarnPoints
                {
                    VirginPoints = basket.EarnPoints.EarnVirginPoints.ToString(),
                    TierPoints = basket.EarnPoints.EarnTierPoints.ToString(),
                    GroupVirginPoints = basket.EarnPoints.EarnGroupVirginPoints.ToString()
                };
            }

            // Tier changed (or no cached values) → recalculate
            basket.EarnPointPerTier = new() { Tiers = [] };

            var eligibleCount = basket.People.Count(p => p.Type != PtcEnums.PtcTypeEnum.INF.ToString());

            var leadResponse = await _inRuleService.AirlinePointsEarnCalculator(InRuleRequestMapper.MapPointsEarnCalculatorRequest(flightSummaryResponse, customerDetails));

            if (leadResponse?.SegmentResponses == null || !leadResponse.SegmentResponses.Any())
            {
                return new EarnPoints
                {
                    VirginPoints = null,
                    TierPoints = null,
                    GroupVirginPoints = null
                };
            }

            var leadVirginPoints = !flightSummaryResponse.IsAwardSearchResult ? leadResponse.SegmentResponses.Sum(p => p.PointsAward) : (int?)null;

            var leadTierPoints = leadResponse.SegmentResponses.Sum(p => p.TierPointsAward);

            int? groupVirginPoints = leadVirginPoints;

            basket.EarnPointPerTier.Tiers = basket.EarnPointPerTier.Tiers.Concat(
            [
                new TierEarnPoints
                {
                    TierType = currentTier,
                    VirginPoints = leadVirginPoints ?? 0,
                    TierPoints = leadTierPoints
                }
            ]);

            if (eligibleCount > 1 && !flightSummaryResponse.IsAwardSearchResult)
            {
                if ((currentTier == "Red" && leadVirginPoints.HasValue) || customerDetails == null)
                {
                    groupVirginPoints = leadVirginPoints * eligibleCount;
                }
                else
                {
                    int? redVirginPoints = null;
                    int? redTierPoints = null;

                    var redPointsResponse = await _inRuleService.AirlinePointsEarnCalculator(InRuleRequestMapper.MapPointsEarnCalculatorRequest(flightSummaryResponse, null));

                    if (redPointsResponse?.SegmentResponses?.Any() == true)
                    {
                        redVirginPoints = redPointsResponse.SegmentResponses.Sum(p => p.PointsAward);
                        redTierPoints = redPointsResponse.SegmentResponses.Sum(p => p.TierPointsAward);
                    }

                    groupVirginPoints = (redVirginPoints * (eligibleCount - 1)) + leadVirginPoints;

                    basket.EarnPointPerTier.Tiers = basket.EarnPointPerTier.Tiers.Concat(
                    [
                        new TierEarnPoints
                        {
                            TierType = "Red",
                            VirginPoints = redVirginPoints ?? 0,
                            TierPoints = redTierPoints ?? 0
                        }
                    ]);
                }
            }

            return new EarnPoints
            {
                VirginPoints = leadVirginPoints?.ToString(),
                TierPoints = leadTierPoints.ToString(),
                GroupVirginPoints = groupVirginPoints?.ToString()
            };
        }
    }
}
