namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class EmailDetail
    {
        public enum EmailUsages
        {
            Business,
            Personal
        }
        public EmailUsages? Usage { get; set; }
        public string? EmailId { get; set; }
        public string? PreferredFlag { get; set; }
        public string? Description { get; set; }
    }
}
