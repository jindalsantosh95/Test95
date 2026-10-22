namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class DebugInfoType
    {
        public string ConfigurationSettings { get; set; }
        public string DataSourceRequest { get; set; }
        public string DataSourceResponse { get; set; }
        public string DataSourceName { get; set; }
        public string TransportInfo { get; set; }
        public string Timers { get; set; }
        public string AdditionalInfo { get; set; }
        public RequestInfoType RequestInfo { get; set; }
    }
}
