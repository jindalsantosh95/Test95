namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class Document
    {
        public string IssueCountry {  get; set; }
        public string NationalityCountry { get; set; }
        public Visa Visa { get; set; }
        public string ExpirationDate { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
    }
}
