

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class OriginalPOS
    {
        public string PartyName { get; set; }
        public string IATANumber { get; set; }
        public string AgencyId { get; set; }
        public string AgentId { get; set; }
        public string PCC { get; set; }
        public string CityCode { get; set; }
        public string GDSPSSIndentifier { get; set; }
        public string CountryCode { get; set; }
        public Device Device { get; set; }
    }
}
