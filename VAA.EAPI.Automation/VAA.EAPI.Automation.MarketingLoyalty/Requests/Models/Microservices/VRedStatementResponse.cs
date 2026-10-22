using Newtonsoft.Json;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices
{
    public class VRedStatementResponse
    {
        public IEnumerable<Statement> statements { get; set; }
    }

    public class Statement
    {
        public string id { get; set; }

        public DateTime activityDate { get; set; }

        public DateTime transactionDate { get; set; }

        public string description { get; set; }

        public string type { get; set; }

        public string pointsInt { get; set; }

        [JsonProperty("details")]
        public string DetailsJson { get; set; }

        [JsonIgnore]
        public Details Details { get; set; }

        public VredDetail vredDetail { get; set; }
    }

    public class Details
    {
        public bool isFlight { get; set; }
        public long? accruedValueMiles { get; set; }
        public string action { get; set; }
        public DateTime activityDatetime { get; set; }
        public string activityDifferentiator { get; set; }
        public int? activityMultiplier { get; set; }
        public string activityNumber { get; set; }
        public string activitySubtype { get; set; }
        public string activityType { get; set; }
        public long? basePoints { get; set; }
        public long? bonusMiles { get; set; }
        public string bookingClassCode { get; set; }
        public string bookingReference { get; set; }
        public string cancellationReason { get; set; }
        public string cancelledBy { get; set; }
        public string cancelledTxnId { get; set; }
        public string classCode { get; set; }
        public int? classOfServicePoints { get; set; }
        public string comments { get; set; }
        public DateTime? createdDatetime { get; set; }
        public string createdBy { get; set; }
        public string creditSourceType { get; set; }
        public int? eliteTierBonusPoints { get; set; }
        public string fareBasis { get; set; }
        public int? giftAdjustments { get; set; }
        public string id { get; set; }
        public string info { get; set; }
        public DateTime? insertedDatetime { get; set; }
        public bool manualAdjustmentFlag { get; set; }
        public long? membershipNumber { get; set; }
        public int? milesPlusCashNetMiles { get; set; }
        public int? nonGiftAdjustments { get; set; }
        public int? numUnits { get; set; }
        public string owner { get; set; }
        public string pannNumber { get; set; }
        public string partnerCategory { get; set; }
        public string partnerCode { get; set; }
        public string partnerName { get; set; }
        public string partnerSubtype { get; set; }
        public string pdmFileType { get; set; }
        public string pdmPartnerFilename { get; set; }
        public long? points { get; set; }
        public IEnumerable<string>? pointsComponents { get; set; }
        public string postingType { get; set; }
        public string productDescription { get; set; }
        public string productCategory { get; set; }
        public string productName { get; set; }
        public int? promotionBonusPoints { get; set; }
        public int? quantity { get; set; }
        public bool retroIndicatorFlag { get; set; }
        public bool retroOverrideFlag { get; set; }
        public string retroOverrideReason { get; set; }
        public string schema { get; set; }
        public IEnumerable<Sector>? sectors { get; set; }
        public int? standardMiles { get; set; }
        public string statementText { get; set; }
        public string status { get; set; }
        public string ticketIssueDatetime { get; set; }
        public string transactionChannelId { get; set; }
        public DateTime? transactionDatetime { get; set; }
        public string transactionId { get; set; }
        public string transactionStatus { get; set; }
        public long? transferMembershipNumber { get; set; }
        public int? travelMiles { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedDatetime { get; set; }
        public string vapDMDocNumber { get; set; }
        public string vapDMRecordId { get; set; }
    }

    public class VredDetail
    {
        public string transactionId { get; set; }
        public string partnerName { get; set; }
        public string partnerCode { get; set; }
        public string partnerSubtype { get; set; }
        public bool isFlight { get; set; }
        public DateTime? activityDatetime { get; set; }
        public string activitySubtype { get; set; }
        public string activityType { get; set; }
        public string comments { get; set; }
        public string createdBy { get; set; }
        public string membershipNumber { get; set; }
        public string owner { get; set; }
        public string points { get; set; }
        public string productName { get; set; }
        public string statementText { get; set; }
        public string transactionChannelId { get; set; }
        public string transactionStatus { get; set; }
        public DateTime? transactionDatetime { get; set; }
    }

    public class Sector
    {
        public string cabinClass { get; set; }
        public string destinationAirport { get; set; }
        public string flownBookingClass { get; set; }
        public string marketingBookingClass { get; set; }
        public string marketingCarrierCode { get; set; }
        public string marketingFlightNumber { get; set; }
        public string operatingFlightNumber { get; set; }
        public string originAirport { get; set; }
        public int? points { get; set; }
        public string sectorCityPair { get; set; }
        public string serviceClass { get; set; }
        public string ticketNumber { get; set; }

    }
}
