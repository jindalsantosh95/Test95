using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Domain
{
    public class OrderCreateRequest
    {
        public Pos pos { get; set; }
        public Parties parties { get; set; }
        public List<SelectedOffer> selectedOffers { get; set; }
        public List<PaymentDetails> paymentDetails { get; set; }
        public DataList dataList { get; set; }
    }
}