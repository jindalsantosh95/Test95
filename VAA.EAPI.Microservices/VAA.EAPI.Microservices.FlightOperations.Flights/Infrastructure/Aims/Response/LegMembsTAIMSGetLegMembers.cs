namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
    public partial class LegMembsTAIMSGetLegMembers
    {
        public byte FCount { get; set; }
        [System.Xml.Serialization.XmlArrayItem("TAIMSMember", IsNullable = false)]
        public LegMembsTAIMSGetLegMembersTAIMSMember[] FMember { get; set; }
        public string FlightCarrier { get; set; }
        public byte FlightDD { get; set; }
        public string FlightDep { get; set; }
        public string FlightDesc { get; set; }
        public string FlightLegCD { get; set; }
        public byte FlightMM { get; set; }
        public string FlightNo { get; set; }
        public ushort FlightYY { get; set; }
    }
}
