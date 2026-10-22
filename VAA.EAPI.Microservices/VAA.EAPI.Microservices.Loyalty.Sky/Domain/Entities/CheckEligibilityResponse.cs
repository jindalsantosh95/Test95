namespace VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities
{
    public class CheckEligibilityResponse
    {
        public string TrackID { get; set; }

        public string EligibilityDenialCode { get; set; }

        public SkyClaimResponse SkyClaimResponse { get; set; }

        public bool IsEligible { get; set; }
    }
}
