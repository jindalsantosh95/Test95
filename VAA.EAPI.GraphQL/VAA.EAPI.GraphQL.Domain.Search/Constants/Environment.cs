using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Search.Constants
{
    public class Environment
    {
        public static readonly string SearchPostBackUrl = "Search:PostBackUrl";
        public static readonly string SearchOfferMsUrl = "Search:OfferMS:URL";
        public static readonly string SearchOfferMsOcpKey = "Search:OfferMS:OCPKey";
        public static readonly string SearchOfferMsChannelId = "Search:OfferMS:ChannelId";


        public static readonly string BrandMsUrl = "Search:BrandMS:URL";
        public static readonly string BrandMsOcpKey = "Search:BrandMS:OCPKey";

        public static readonly string BasketMSCreateUrl = "BasketMSCreateUrl";
        public static readonly string BasketMSOcpKey = "BasketMSOcpKey";
        public static readonly string BasketMSUpdateOfferIDUrl = "BasketMSUpdateOfferIDUrl";
        public static readonly string BasketMSUpdateSelectedOfferUrl = "BasketMSUpdateSelectedOfferUrl";
        public static readonly string BasketMSRetrieveUrl = "BasketMSRetrieveUrl";
        public static readonly string BasketMSSetFlightSummaryUrl = "BasketMSSetFlightSummaryUrl";
        public static readonly string BasketMSUpdateMembershipUrl = "BasketMSUpdateMembershipUrl";

        public static readonly string ReferenceMSOcpKey = "ReferenceMSOcpKey";
        public static readonly string BaggageSummaryMockResponse = "Search:BaggageSummaryMockResponse";

        public static readonly string SustainMsUrl = "Search:SustainMS:URL";
        public static readonly string SustainMSOcpKey = "Search:SustainMS:OCPKey";
        public static readonly bool EnableCarbonIndexIntegration = ConfigReader.Try("Search:EnableCarbonIndexIntegration", true);

        public static readonly bool EnableErrorHandling = ConfigReader.Try("EnableErrorHandling", true);
        public static readonly string EnableEarnPointCalculator = "EnableEarnPointCalculator";
        public static readonly string EnableVoucherRedemption = "EnableVoucherRedemption";
        public static readonly string EnableRewardSeatAvailability = "EnableRewardSeatAvailability";
        public static readonly string RewardSeatOfferCalendarEndpoint = "RewardSeatAvailability:OfferCalendarEndpoint";

        public static readonly string RewardSeatAvailabilityMaxAdvanceDays = "RewardSeatAvailability:MaxAdvanceDays";
        public static readonly string RewardSeatAvailabilityMaxWorkUnits = "RewardSeatAvailability:MaxWorkUnits";
        public static readonly string RewardSeatAvailabilityMaxConcurrency = "RewardSeatAvailability:MaxConcurrency";
        public static readonly string RewardSeatAvailabilityOverallTimeoutSeconds = "RewardSeatAvailability:OverallTimeoutSeconds";
        public static readonly string RewardSeatAvailabilityFareFamilyMappingVersion = "RewardSeatAvailability:FareFamilyMappingVersion";
        public static readonly string RewardSeatAvailabilityOfferCacheTtl = "RewardSeatAvailability:OfferCacheTtl";
        public static readonly string RewardSeatAvailabilityItaVersion = "RewardSeatAvailability:ItaVersion";
        public static readonly string AirportInfoRoutesApi = "AirportInfoQuery:RoutesApi";
        public static string RewardSeatDailySeatCap(string cabin) => $"RewardSeatAvailability:SeatCaps:{cabin}:Daily";
        public static string RewardSeatMonthlySeatCap(string cabin) => $"RewardSeatAvailability:SeatCaps:{cabin}:Monthly";
    }
}
