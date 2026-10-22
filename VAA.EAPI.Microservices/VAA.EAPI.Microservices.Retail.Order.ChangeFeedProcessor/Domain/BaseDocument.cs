using Microsoft.Azure.Documents;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public class BaseDocument : Document
    {
        public string DocType { get; set; }
    }
}
