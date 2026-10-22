namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.ValidatePnr
{
    public class IdentityAcquireTokenRequest
    {
        public string? Type { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? RLOC { get; set; }
        public string? TicketRef { get; set; }
    }
}
