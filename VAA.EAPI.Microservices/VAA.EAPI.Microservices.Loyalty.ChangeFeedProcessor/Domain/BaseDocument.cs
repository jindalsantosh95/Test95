using Microsoft.Azure.Documents;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain
{
    public class BaseDocument : Document
    {
        public string DocType { get; set; }
    }
}
