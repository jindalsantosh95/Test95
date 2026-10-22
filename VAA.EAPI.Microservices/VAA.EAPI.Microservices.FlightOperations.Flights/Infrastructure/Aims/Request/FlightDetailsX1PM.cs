namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Request
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://tempuri.org/")]
    public partial class FlightDetailsX1PM
    {
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int DepArrMode { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public object FlightACType { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public object FlightArr { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public object FlightCarrier { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public object FlightDep { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public object FlightLegCd { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public object FlightNum { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int FromDD { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int FromHH { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int FromMin { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int FromMonth { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int FromYYYY { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int ToDD { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int ToHH { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int ToMin { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int ToMonth { get; set; }
        [System.Xml.Serialization.XmlElement(Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public int ToYYYY { get; set; }
    }
}
