namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement
{
    public class EditVSRetroFlownDataRequest
    {
        public string OriginAirport { get; set; }

        public string DestinationAirport { get; set; }

        public string TicketNumber { get; set; }

        public string Status { get; set; }

        public string VALoyaltyProgramCode { get; set; }

        public string MemberNumber { get; set; }
    }
}
