namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class SecureFlightPassengerDataType
    {
        public string AuthorizationToRetainSFPD { get; set; }
        public string BirthDay { get; set; }
        public string BirthMonth { get; set; }
        public string BirthYear { get; set; }
        public string DocumentExpirationDateDay { get; set; }
        public string DocumentExpirationDateMonth { get; set; }
        public string DocumentExpirationDateYear { get; set; }
        public string DocumentIssuingCountry { get; set; }
        public string DocumentIssuingState { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentType { get; set; }
        public string Gender { get; set; }
        public string GivenName { get; set; }
        public string IDVerifiedByAgentIndicator { get; set; }
        public string MethodOfDataEntry { get; set; }
        public string MiddleName { get; set; }
        public string SourceOfSFPDOnHost { get; set; }
        public string SurName { get; set; }
    }
}
