namespace VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities
{
    public class SkyOnlineRetroFeedbackResponse
    {
        public string TrackID { get; set; }

        public string EligibilityDenialCode { get; set; }

        public string UniqueIdentificationNumber { get; set; }

        public bool IsSuccess { get; set; }
    }
}
