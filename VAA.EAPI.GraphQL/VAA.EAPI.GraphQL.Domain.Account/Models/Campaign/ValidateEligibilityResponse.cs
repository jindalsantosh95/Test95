namespace VAA.EAPI.GraphQL.Domain.Account.Models.Campaign
{
    public class ValidateEligibilityResponse
    {
        public bool Valid { get; set; }
        public bool Claimed { get; set; }
        public string FcNumber { get; set; }
        public string ClaimedDateTime { get; set; }
    }
}
