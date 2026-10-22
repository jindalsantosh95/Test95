using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using static VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class OfferDocument : CosmosDocument
    {
        public string OrderId { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public string ShoppingResponseId { get; set; }
        public string DocType { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string MetaPartnerId { get; set; }
        public FlightOfferResponse FlightOfferResponse { get; set; }
        public PaxNameChange PaxNameChangeDetails { get; set; }
        public FormOfPayment? FormOfPayments { get; set; }
    }
}
