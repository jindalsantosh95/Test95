using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.NDC.Offer.Domain.Model.OfferRQ;
using static VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS
{
    public class OfferResponse
    {
        public string OrderId { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public string ShoppingResponseId { get; set; }
        public int TotalOffers { get; set; }
        public IEnumerable<OfferDetails> Offers { get; set; }
        public DataList DataList { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
        public bool NoChangeId { get; set; }
        public FormOfPayment? FormOfPayments { get; set; }
    }
}

