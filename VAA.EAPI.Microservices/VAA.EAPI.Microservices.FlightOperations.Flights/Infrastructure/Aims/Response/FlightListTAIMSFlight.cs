namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims.Response
{
    [Serializable()]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
    public partial class FlightListTAIMSFlight
    {
        public string FlightAcType { get; set; }
        public string FlightActualDayArr { get; set; }
        public string FlightActualDayDep { get; set; }
        public string FlightArr { get; set; }
        public object FlightArrDelays { get; set; }
        public object FlightAssocCrwRtes { get; set; }
        public string FlightAta { get; set; }
        public string FlightAtd { get; set; }
        public string FlightBlkTime { get; set; }
        public string FlightCarrier { get; set; }
        public int FlightDD { get; set; }
        public string FlightDate { get; set; }
        public string FlightDep { get; set; }
        public object FlightDepDelays { get; set; }
        public string FlightDesc { get; set; }
        public string FlightEta { get; set; }
        public string FlightEtd { get; set; }
        public string FlightLegCD { get; set; }
        public int FlightMM { get; set; }
        public int FlightNo { get; set; }
        public int FlightNoOfPax { get; set; }
        public string FlightReg { get; set; }
        public string FlightSta { get; set; }
        public string FlightStatus { get; set; }
        public string FlightStd { get; set; }
        public string FlightTDOWN { get; set; }
        public string FlightTKOFF { get; set; }
        public string FlightType { get; set; }
        public int FlightYY { get; set; }
    }
}
