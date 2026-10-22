using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy
{

    [XmlRoot(ElementName = "error")]
    public class DeltaErrorResponse
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public string DeveloperMessage { get; set; }

        public IEnumerable<Info> MoreInfo { get; set; }
    }



}
