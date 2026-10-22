namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro
{
    public class ValidateVSClaimInputsRequest
    {
        public string? TicketDesignator { get; set; }

        public string? FareBasis { get; set; }

        public string? PassengerType { get; set; }
    }
}
