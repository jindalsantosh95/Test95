namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class SkyMSSuccessResponse : SkyPartnerRetroResponse
    {
        public string TrackID { get; set; }
        public string EligibilityDenialCode { get; set; }
        public string UniqueIdentificationNumber { get; set; }
        public bool IsSuccess { get; set; }
    }
}
