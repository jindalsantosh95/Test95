namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
    public partial class FMemberTAIMSMember
    {
        public byte CrewlegOrder { get; set; }
        public string Flt { get; set; }
        public string PayrollNumber { get; set; }
        public string ac { get; set; }
        public string @base { get; set; }
        public ushort crsday { get; set; }
        public string crsdaydd { get; set; }
        public string crsdaymm { get; set; }
        public string crsdayyy { get; set; }
        public string crte { get; set; }
        public string firstName { get; set; }
        public int id { get; set; }
        public string lastName { get; set; }
        public string name { get; set; }
        public string notif { get; set; }
        public string pax { get; set; }
        public string pos { get; set; }
        public string qualcat { get; set; }
        public string request { get; set; }
        public string shortName { get; set; }
        public string trnduty { get; set; }
        public string FlightLegCd { get; set; }
    }
}
