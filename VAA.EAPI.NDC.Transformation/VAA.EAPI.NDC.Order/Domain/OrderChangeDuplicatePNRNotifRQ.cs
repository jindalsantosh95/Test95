using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VAA.EAPI.NDC.Order.Domain
{
    [XmlRoot(ElementName = "QueueElem",
          Namespace = "http://wse.sabre.com/eventing")]
    public class OrderChangeDuplicatePNRNotifRQ
    {
        [XmlElement(ElementName = "UniqueId")]
        public string UniqueId { get; set; }

        [XmlElement(ElementName = "PartitionCode")]
        public string PartitionCode { get; set; }

        [XmlElement(ElementName = "PCC")]
        public string PCC { get; set; }

        [XmlElement(ElementName = "QueueNo")]
        public string QueueNo { get; set; }

        [XmlElement(ElementName = "PrefactoryCode")]
        public string PrefactoryCode { get; set; }

        [XmlElement(ElementName = "QueueDepth")]
        public string QueueDepth { get; set; }
    }
}
