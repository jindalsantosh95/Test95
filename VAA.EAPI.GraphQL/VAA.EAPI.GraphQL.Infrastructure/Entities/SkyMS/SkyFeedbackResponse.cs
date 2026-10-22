namespace VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS
{
    public class SkyFeedbackResponse
    {
        public string TrackID { get; set; }

        public string EligibilityDenialCode { get; set; }

        public string UniqueIdentificationNumber { get; set; }

        public bool IsSuccess { get; set; }
    }
}
