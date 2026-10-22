namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class GovtIssuedNumberType1
    {
        public string AuthorizationToRetainGovtNum { get; set; }
        public string DocumentApplicableCountry { get; set; }
        public string DocumentApplicableState { get; set; }
        public string DocumentIssueDay { get; set; }
        public string DocumentIssueMonth { get; set; }
        public string DocumentIssueYear { get; set; }
        public string GovtIssuedNumber { get; set; }
        public string GovtIssuedNumberType { get; set; }
        public string InfantIndicator { get; set; }
        public string PlaceOfBirth { get; set; }
        public string PlaceOfIssue { get; set; }
        public string SourceOfGovtNumberOnHost { get; set; }
    }
}
