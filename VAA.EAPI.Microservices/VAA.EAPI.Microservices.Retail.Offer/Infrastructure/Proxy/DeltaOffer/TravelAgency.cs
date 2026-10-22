namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class TravelAgency
    {
        public string TravelAgencyId { get; set; }
        public string TravelAgencyName { get; set; }
        public int AgencyControlNum { get; set; }
        public string PseudoCityId { get; set; }
        public string AgencyTypeCode { get; set; }
        public TravelAgent TravelAgent { get; set; }
    }
}
