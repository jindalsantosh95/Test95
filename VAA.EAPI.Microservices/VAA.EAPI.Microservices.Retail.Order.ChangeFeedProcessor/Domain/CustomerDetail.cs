using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class CustomerDetail
    {
        [JsonProperty("VAASCVID")]
        public string? VAASCVID { get; set; }

        [JsonProperty("ContactTypeText")]
        public string? ContactTypeText { get; set; }

        [JsonProperty("Title")]
        public string? Title { get; set; }

        [JsonProperty("CustID")]
        public string? CustId { get; set; }

        [JsonProperty("PTC")]
        public string? Ptc { get; set; }

        [JsonProperty("FirstName")]
        public string? FirstName { get; set; }

        [JsonProperty("LastName")]
        public string? LastName { get; set; }

        [JsonProperty("MiddleName")]
        public string? MiddleName { get; set; }

        [JsonProperty("FirstNameNumber")]
        public string? FirstNameNumber { get; set; }

        [JsonProperty("LastNameNumber")]
        public string? LastNameNumber { get; set; }

        [JsonProperty("BookingType")]
        public string? BookingType { get; set; }

        [JsonProperty("FrequentFlyer")]
        public FrequentFlyer? FrequentFlyer { get; set; }

        [JsonProperty("CustomerType")]
        public string? CustomerType { get; set; }

        [JsonProperty("CustomerSubType")]
        public string? CustomerSubType { get; set; }

        [JsonProperty("CustomerValue")]
        public string? CustomerValue { get; set; }

        [JsonProperty("CustomerRiskValue")]
        public string? CustomerRiskValue { get; set; }

        [JsonProperty("CustomerLifeTimeValue")]
        public string? CustomerLifeTimeValue { get; set; }

        [JsonProperty("VirginHolidayBookingFlag")]
        public string? VirginHolidayBookingFlag { get; set; }

        [JsonProperty("CorporateIndicator")]
        public string? CorporateIndicator { get; set; }

        [JsonProperty("CorporateAccountName")]
        public string? CorporateAccountName { get; set; }

        [JsonProperty("StaffTravellerIndicator")]
        public string? StaffTravellerIndicator { get; set; }

        [JsonProperty("StaffTravellerType")]
        public string? StaffTravellerType { get; set; }

        [JsonProperty("DocumentType")]
        public string? DocumentType { get; set; }

        [JsonProperty("DocumentNumber")]
        public string? DocumentNumber { get; set; }

        [JsonProperty("CountryOfIssue")]
        public string? CountryOfIssue { get; set; }

        [JsonProperty("GenderOnDocument")]
        public string? GenderOnDocument { get; set; }

        [JsonProperty("DocumentFirstName")]
        public string? DocumentFirstName { get; set; }

        [JsonProperty("DocumentMiddleName")]
        public string? DocumentMiddleName { get; set; }

        [JsonProperty("DocmumentLastName")]
        public string? DocmumentLastName { get; set; }

        [JsonProperty("DocumentTitle")]
        public string? DocumentTitle { get; set; }

        [JsonProperty("DOBOnDocument")]
        public string? DobOnDocument { get; set; }

        [JsonProperty("Nationality")]
        public string? Nationality { get; set; }

        [JsonProperty("ExpiryDate")]
        public string? ExpiryDate { get; set; }

        [JsonProperty("CustomerService")]
        public IEnumerable<CustomerService>? CustomerService { get; set; }

        [JsonProperty("CustomerContacts")]
        public CustomerContacts CustomerContacts { get; set; }

        [JsonProperty("HealthPassport")]
        public HealthPassport? HealthPassport { get; set; }
    }
}
