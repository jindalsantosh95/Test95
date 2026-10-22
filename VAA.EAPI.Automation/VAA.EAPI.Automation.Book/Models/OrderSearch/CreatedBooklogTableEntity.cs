using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Automation.Book.Models.OrderSearch
{
    public class CreatedBooklogTableEntity : TableEntity
    {
        public string PaymentCardType { get; set; }
        public string CardNumber { get; set; }
        public string PRI { get; set; }
        public string BasketId { get; set; }
        public string Response { get; set; }
        public string Payload { get; set; }
        public string OrderNo { get; set; }
        public string Journey { get; set; }
        public string ThreeDS { get; set; }
        public string Env { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
    }
}
