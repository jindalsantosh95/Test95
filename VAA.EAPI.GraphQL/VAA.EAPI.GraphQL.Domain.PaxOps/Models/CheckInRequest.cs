namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    public class CheckInRequest
    {
        public string PnrLocator { get; set; }
        public string ETicketNumber { get; set; }
        public string OriginCode { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
    }
}