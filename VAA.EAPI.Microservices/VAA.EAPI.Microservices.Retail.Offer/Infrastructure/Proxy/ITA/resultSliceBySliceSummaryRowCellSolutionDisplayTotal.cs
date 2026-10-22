using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy
{

    // XmlSerializer sets this flag only when the generated numeric attribute is present.
    public partial class resultSliceBySliceSummaryRowCellSolutionDisplayTotal
    {
        [XmlIgnore]
        public bool amountSpecified { get; set; }
    }
}
