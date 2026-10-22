namespace VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS
{
    public class CheckEligibilityClaimResponse
    {
        public string TrackID { get; set; }

        public string EligibilityDenialCode { get; set; }

        public SkyClaimResponse SkyClaimResponse { get; set; }

        public bool IsEligible { get; set; }
    }
}
