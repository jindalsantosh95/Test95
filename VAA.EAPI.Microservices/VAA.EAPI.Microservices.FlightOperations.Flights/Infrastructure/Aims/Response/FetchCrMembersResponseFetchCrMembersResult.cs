namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    public partial class FetchCrMembersResponseFetchCrMembersResult
    {
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int Count { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int CrouteDD { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int CrouteMM { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int CrouteYY { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService", IsNullable = true)]
        public string ErrorExplanation { get; set; }
        [System.Xml.Serialization.XmlArray(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        [System.Xml.Serialization.XmlArrayItem("TAIMSMember", IsNullable = false)]
        public FMemberTAIMSMember[] FMember { get; set; }
    }
}
