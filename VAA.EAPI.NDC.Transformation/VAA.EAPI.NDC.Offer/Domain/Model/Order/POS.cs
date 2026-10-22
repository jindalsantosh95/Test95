namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class POS
    {
        public string PartyName { get; set; }
        public string IATANumber { get; set; }
        public string AgencyId { get; set; }
        public string AgentId { get; set; }
        public string PCC { get; set; }
        public string CityCode { get; set; }
        public string CurrencyCode { get; set; }
        public string GDSPSSIndentifier { get; set; }
        public string CountryCode { get; set; }
        public POSDevice Device { get; set; }
        public string DefaultCityCode { get; set; }
    }
}
