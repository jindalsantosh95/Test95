using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Insurance
{
    public class Offer
    {
        public string ProductCode { get; set; }
        public string QuoteId { get; set; }
        public string ContentCode { get; set; }
        public string ContentType { get; set; }
        public string Id { get; set; }
        public int Sequence { get; set; }
        public Price PriceDetails { get; set; }
        public IEnumerable<ProductSpecificMessage> ProductSpecificMessages { get; set; }
        public IEnumerable<Message> Messages { get; set; }
        public string HtmlOutput { get; set; }

    }
}