using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS
{
    public class PaymentTransactionDocument : CosmosDocument
    {
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<Payment> PaymentDetails { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public string JourneyType { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public string Status { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
}
