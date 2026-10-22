namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class FeedbackRequest
    {
        public string UniqueIdentificationNumber { get; set; }
        public string FFPCarrierCode { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string PostingStatus { get; set; }
        public string BaseMiles { get; set; }
        public string PostingDate { get; set; }
        public string ClassOfServiceBonusMiles { get; set; }
        public string EliteTierBonusMiles { get; set; }
        public string PromotionalBonusMiles { get; set; }
    }
}
