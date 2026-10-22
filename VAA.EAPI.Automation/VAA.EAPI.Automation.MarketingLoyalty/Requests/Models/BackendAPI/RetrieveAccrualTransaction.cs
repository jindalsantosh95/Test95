namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.BackendAPI
{
    public class AccrualTransaction
    {
        public string MemberNumber { get; set; }
        public string TransactionType { get; set; }
        public string TransactionProcessedDate { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string TransactionChannel { get; set; }
        public string ActivityDate { get; set; }
        public FlightDetails FlightDetail { get; set; }
        public List<CreditItem> CreditItems { get; set; }
    }
    public class CreditItem
    {
        public string CreditedValue { get; set; }

        public string PointType { get; set; }

        public string PointSubType { get; set; }
    }

    public class FlightDetails
    {
        public string BookingReference { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }

    }




}
