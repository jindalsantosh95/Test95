namespace VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS
{
    public class TicketStatusRequest
    {
        public string FlownRecordId { get; set; }
        public string TicketNumber { get; set; }
        public string TicketStatus { get; set; }
        public string? MemberNumber { get; set; }
        public string? LoyaltyProgrammeCode { get; set; }
        public string? PostingTypeCode { get; set; }
    }
}
