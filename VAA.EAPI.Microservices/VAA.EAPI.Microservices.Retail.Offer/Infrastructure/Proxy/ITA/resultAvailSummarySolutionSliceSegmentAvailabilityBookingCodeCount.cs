using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy
{

    // XmlSerializer sets this flag only when the generated numeric attribute is present.
    public partial class resultAvailSummarySolutionSliceSegmentAvailabilityBookingCodeCount
    {
        [XmlIgnore]
        public bool countSpecified { get; set; }
    }
}
