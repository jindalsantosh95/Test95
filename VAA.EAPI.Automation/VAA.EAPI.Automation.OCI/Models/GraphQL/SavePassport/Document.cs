namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SavePassport
{
    public class Document
    {
        public string documentType { get; set; }
        public string givenNames { get; set; }
        public string surname { get; set; }
        public string dateOfBirth { get; set; }
        public Gender gender { get; set; }
        public string documentNumber { get; set; }
        public string expirationDate { get; set; }
        public IssuingCountry issuingCountry { get; set; }
        public Nationality nationality { get; set; }
        public CountryOfResidence countryOfResidence { get; set; }
    }
}