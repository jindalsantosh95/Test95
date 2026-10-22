namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
    public partial class LegMembsTAIMSGetLegMembersTAIMSMember
    {
        public string PayrollNumber { get; set; }
        public string ac { get; set; }
        public string @base { get; set; }
        public ushort crsday { get; set; }
        public byte crsdaydd { get; set; }
        public byte crsdaymm { get; set; }
        public ushort crsdayyy { get; set; }
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
    }
}
