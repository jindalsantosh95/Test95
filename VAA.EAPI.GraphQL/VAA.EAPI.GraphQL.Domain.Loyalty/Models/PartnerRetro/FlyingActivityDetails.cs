namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class FlyingActivityDetails : SkyPartnerRetroResponse
    {
        public string TrackID { get; set; }
        public string EligibilityDenialCode { get; set; }
        public SkyClaimResponse SkyClaimResponse { get; set; }
        public bool IsEligible { get; set; }
    }
}
