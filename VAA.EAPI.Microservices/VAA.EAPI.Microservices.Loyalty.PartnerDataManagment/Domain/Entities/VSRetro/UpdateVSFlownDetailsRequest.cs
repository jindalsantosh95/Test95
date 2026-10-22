namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro
{
    public class UpdateVSFlownDetailsRequest
    {
        public string OriginAirport { get; set; } = string.Empty;
        public string DestinationAirport { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? VALoyaltyProgramCode { get; set; }
        public string? MemberNumber { get; set; }
    }
}
