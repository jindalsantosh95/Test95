using VAA.EAPI.Automation.OCI.Models.GraphQL.SavePassport;

namespace VAA.EAPI.Automation.OCI.Models.Delta
{
    public class IdentityDocument
    {
        public string type { get; set; }
        public string givenNames { get; set; }
        public string middleName { get; set; }
        public string surname { get; set; }
        public string birthDate { get; set; }
        public Gender gender { get; set; }
        public string number { get; set; }
        public string expirationDate { get; set; }
        public IssuingCountry issuingCountry { get; set; }
        public Nationality nationality { get; set; }
        public CountryOfResidence countryOfResidence { get; set; }
        public string dataSourceType { get; set; }
    }
}