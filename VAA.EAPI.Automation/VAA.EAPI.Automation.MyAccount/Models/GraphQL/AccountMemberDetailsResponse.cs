using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MyAccount.Models.GraphQL
{
    public sealed class AccountMemberDetailsResponse
    {
        [JsonProperty("data")]
        public AccountMemberDetailsResponseData? Data { get; set; }
    }

    public sealed class AccountMemberDetailsResponseData
    {
        [JsonProperty("accountMemberDetails")]
        public AccountMemberDetails? AccountMemberDetails { get; set; }
    }

    public sealed class AccountMemberDetails
    {
        [JsonProperty("voucherResponse")]
        public VoucherResponse? VoucherResponse { get; set; }

        [JsonProperty("flightDetails")]
        public FlightDetails? FlightDetails { get; set; }

        [JsonProperty("activityResponse")]
        public ActivityResponse? ActivityResponse { get; set; }

        [JsonProperty("flyingClubMemberDetails")]
        public FlyingClubMemberDetails? FlyingClubMemberDetails { get; set; }

        [JsonProperty("vouchersCount")]
        public int? VouchersCount { get; set; }

        [JsonProperty("activitiesCount")]
        public int? ActivitiesCount { get; set; }

        [JsonProperty("flightsCount")]
        public int? FlightsCount { get; set; }
    }

    public sealed class VoucherResponse
    {
        [JsonProperty("voucherDetails")]
        [JsonConverter(typeof(SingleOrArrayConverter<VoucherData>))]
        public List<VoucherData>? VoucherDetails { get; set; }

        [JsonProperty("errorResponse")]
        public object? ErrorResponse { get; set; }
    }

    public sealed class VoucherData
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("source")]
        public string? Source { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("issueDate")]
        public string? IssueDate { get; set; }

        [JsonProperty("expiryDate")]
        public string? ExpiryDate { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("number")]
        public string? Number { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("isGifted")]
        public bool? IsGifted { get; set; }

        [JsonProperty("points")]
        public string? Points { get; set; }
    }

    internal sealed class SingleOrArrayConverter<TItem> : JsonConverter<List<TItem>>
    {
        public override List<TItem>? ReadJson(JsonReader reader, Type objectType, List<TItem>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            Newtonsoft.Json.Linq.JToken token = Newtonsoft.Json.Linq.JToken.Load(reader);

            if (token.Type == Newtonsoft.Json.Linq.JTokenType.Array)
            {
                return token.ToObject<List<TItem>>(serializer);
            }

            TItem? item = token.ToObject<TItem>(serializer);
            if (item == null)
            {
                return null;
            }

            return new List<TItem> { item };
        }

        public override void WriteJson(JsonWriter writer, List<TItem>? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public sealed class FlightDetails
    {
        [JsonProperty("flightSegmentDetails")]
        public FlightSegmentDetail[]? FlightSegmentDetails { get; set; }

        [JsonProperty("errorResponse")]
        public object? ErrorResponse { get; set; }
    }

    public sealed class FlightSegmentDetail
    {
        [JsonProperty("operatingAirlineName")]
        public string? OperatingAirlineName { get; set; }

        [JsonProperty("dateOfFlight")]
        public string? DateOfFlight { get; set; }

        [JsonProperty("scheduledDepartureTime")]
        public string? ScheduledDepartureTime { get; set; }

        [JsonProperty("originAirportName")]
        public string? OriginAirportName { get; set; }

        [JsonProperty("originAirportCountry")]
        public string? OriginAirportCountry { get; set; }

        [JsonProperty("originAirportCode")]
        public string? OriginAirportCode { get; set; }

        [JsonProperty("destinationAirportName")]
        public string? DestinationAirportName { get; set; }

        [JsonProperty("destinationAirportCountry")]
        public string? DestinationAirportCountry { get; set; }

        [JsonProperty("destinationAirportCode")]
        public string? DestinationAirportCode { get; set; }

        [JsonProperty("flightNumber")]
        public string? FlightNumber { get; set; }

        [JsonProperty("operatingAirlineCode")]
        public string? OperatingAirlineCode { get; set; }

        [JsonProperty("numberOfDaysToDeparture")]
        public int? NumberOfDaysToDeparture { get; set; }

        [JsonProperty("bookingReference")]
        public string? BookingReference { get; set; }

        [JsonProperty("showClaimsBanner")]
        public bool? ShowClaimsBanner { get; set; }

        [JsonProperty("virginPoints")]
        public object? VirginPoints { get; set; }

        [JsonProperty("tierPoints")]
        public object? TierPoints { get; set; }
    }

    public sealed class ActivityResponse
    {
        [JsonProperty("activityDetails")]
        public ActivityDetail[]? ActivityDetails { get; set; }

        [JsonProperty("errorResponse")]
        public object? ErrorResponse { get; set; }
    }

    public sealed class ActivityDetail
    {
        [JsonProperty("activityDate")]
        public DateTime? ActivityDate { get; set; }

        [JsonProperty("transactionProcessedDate")]
        public DateTime? TransactionProcessedDate { get; set; }

        [JsonProperty("partnerName")]
        public string? PartnerName { get; set; }

        [JsonProperty("statement")]
        public string? Statement { get; set; }

        [JsonProperty("virginPoints")]
        public string? VirginPoints { get; set; }

        [JsonProperty("tierPoints")]
        public string? TierPoints { get; set; }

        [JsonProperty("partnerCode")]
        public string? PartnerCode { get; set; }

        [JsonProperty("transactionId")]
        public string? TransactionId { get; set; }

        [JsonProperty("transactionType")]
        public string? TransactionType { get; set; }
    }

    public sealed class FlyingClubMemberDetails
    {
        [JsonProperty("memberType")]
        public string? MemberType { get; set; }

        [JsonProperty("firstName")]
        public string? FirstName { get; set; }

        [JsonProperty("lastName")]
        public string? LastName { get; set; }

        [JsonProperty("dateofBirth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonProperty("gender")]
        public string? Gender { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("contactNumber")]
        public string? ContactNumber { get; set; }

        [JsonProperty("countryCode")]
        public string? CountryCode { get; set; }

        [JsonProperty("address")]
        public Address[]? Address { get; set; }

        [JsonProperty("isVAAMarketingPreferred")]
        public bool? IsVaaMarketingPreferred { get; set; }

        [JsonProperty("preferredPartnerFFP")]
        public string? PreferredPartnerFfp { get; set; }

        [JsonProperty("headOfGroupMemberNumber")]
        public string? HeadOfGroupMemberNumber { get; set; }

        [JsonProperty("tierCode")]
        public string? TierCode { get; set; }

        [JsonProperty("preferredName")]
        public string? PreferredName { get; set; }

        [JsonProperty("fCMembershipNumber")]
        public int? FcMembershipNumber { get; set; }

        [JsonProperty("fCJoinDate")]
        public DateTime? FcJoinDate { get; set; }

        [JsonProperty("dateTierPointsReset")]
        public DateTime? DateTierPointsReset { get; set; }

        [JsonProperty("upgradeByDate")]
        public DateTime? UpgradeByDate { get; set; }

        [JsonProperty("tierPoints")]
        public int? TierPoints { get; set; }

        [JsonProperty("tierPointsNeedToReachNextTier")]
        public int? TierPointsNeedToReachNextTier { get; set; }

        [JsonProperty("flownMiles")]
        public int? FlownMiles { get; set; }

        [JsonProperty("totalVirginPoints")]
        public string? TotalVirginPoints { get; set; }

        [JsonProperty("customerID")]
        public string? CustomerId { get; set; }

        [JsonProperty("profileID")]
        public string? ProfileId { get; set; }

        [JsonProperty("retainTierPoints")]
        public int? RetainTierPoints { get; set; }

        [JsonProperty("tierPointsNeedToRetain")]
        public int? TierPointsNeedToRetain { get; set; }

        [JsonProperty("rollingTierPoints")]
        public int? RollingTierPoints { get; set; }

        [JsonProperty("memberStatus")]
        public MemberStatus? MemberStatus { get; set; }

        [JsonProperty("tierThresholdDetails")]
        public TierThresholdDetail[]? TierThresholdDetails { get; set; }
    }

    public sealed class MemberStatus
    {
        [JsonProperty("isWingsMemeber")]
        public bool? IsWingsMember { get; set; }

        [JsonProperty("isLifeTimeGoldMember")]
        public bool? IsLifeTimeGoldMember { get; set; }

        [JsonProperty("isMillionMiler")]
        public bool? IsMillionMiler { get; set; }

        [JsonProperty("is2MillionMiler")]
        public bool? Is2MillionMiler { get; set; }
    }

    public sealed class Address
    {
        [JsonProperty("addressType")]
        public string? AddressType { get; set; }

        [JsonProperty("addressLine1")]
        public string? AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string? AddressLine2 { get; set; }

        [JsonProperty("addressLine3")]
        public string? AddressLine3 { get; set; }

        [JsonProperty("companyName")]
        public string? CompanyName { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("postalCode")]
        public string? PostalCode { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("preferredFlag")]
        public string? PreferredFlag { get; set; }

        [JsonProperty("primary")]
        public string? Primary { get; set; }

        [JsonProperty("countryCode")]
        public string? CountryCode { get; set; }

        [JsonProperty("countryName")]
        public string? CountryName { get; set; }

        [JsonProperty("postCode")]
        public string? PostCode { get; set; }

        [JsonProperty("landlordOrCompanyName")]
        public string? LandlordOrCompanyName { get; set; }

        [JsonProperty("province")]
        public string? Province { get; set; }

        [JsonProperty("addressVerified")]
        public string? AddressVerified { get; set; }

        [JsonProperty("valid")]
        public string? Valid { get; set; }

        [JsonProperty("metroArea")]
        public string? MetroArea { get; set; }
    }

    public sealed class TierThresholdDetail
    {
        [JsonProperty("tierPointsForRetainToGold")]
        public int? TierPointsForRetainToGold { get; set; }

        [JsonProperty("tierPointsForRetainToSilver")]
        public int? TierPointsForRetainToSilver { get; set; }

        [JsonProperty("tierPointsForRedToSilver")]
        public int? TierPointsForRedToSilver { get; set; }

        [JsonProperty("tierPointsForSilverToGold")]
        public int? TierPointsForSilverToGold { get; set; }
    }
}

