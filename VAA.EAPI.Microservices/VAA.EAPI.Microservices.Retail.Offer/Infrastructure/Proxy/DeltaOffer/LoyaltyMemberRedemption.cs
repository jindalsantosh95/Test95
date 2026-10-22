namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class LoyaltyMemberRedemption
    {
        public string AuthorizationCode { get; set; }
        public string AwardFareLevelCode { get; set; }
        public string CurrentDocumentTotalMileCnt { get; set; }
        public string ExchangedDocumentTotalMileCnt { get; set; }
        public string LoyaltyMemberId { get; set; }
        public string LoyaltyProgramTierCode { get; set; }
        public string MilesPlusCashNetMileCnt { get; set; }
        public string NetMileDifferenceCnt { get; set; }
        public string PartiallyFlownItineraryDocumentCode { get; set; }
    }
}
