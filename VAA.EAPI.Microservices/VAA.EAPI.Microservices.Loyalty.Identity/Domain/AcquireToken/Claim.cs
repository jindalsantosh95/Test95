namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken
{
    public class Claim
    {
        public string Type { get; set; }
        public string RLOC { get; set; }
        public string TicketRef { get; set; }
        public string HashKey { get; set; }
        public string CreationDate { get; set; }
    }
}
