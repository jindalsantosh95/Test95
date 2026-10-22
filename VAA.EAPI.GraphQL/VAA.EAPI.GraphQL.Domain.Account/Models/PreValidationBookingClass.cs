namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class PreValidationPartnerData
    {
        public Dictionary<string, string> ListOfValues { get; set; }
        public IEnumerable<PartnerTerms> PartnerTerms { get; set; }
        public IEnumerable<PartnerBookingClass> PartnerBookingClasses { get; set; }
        public Distance Distance { get; set; }
    }
}
