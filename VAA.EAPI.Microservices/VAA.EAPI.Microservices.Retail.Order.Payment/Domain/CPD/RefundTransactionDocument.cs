using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
{
    public class RefundTransactionDocument : CosmosDocument
    {
        public string DocType { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReferences { get; set; }
        public string OrderId { get; set; }
        public IEnumerable<RefundPayment> PaymentDetails { get; set; }
        public IEnumerable<AccountableDocumentRef> AccountableDocumentRefs { get; set; }
        public string Status { get; set; }

        public static explicit operator RefundTransactionDocument(JObject source)
        {
            return new()
            {
                DocType = source["DocType"]?.ToString(),
                CreatedOn = source["CreatedOn"]?.ToString().Parse<DateTime>().ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = source["CreatedBy"]?.ToString(),
                ModifiedBy = source["ModifiedBy"]?.ToString(),
                ModifiedOn = source["ModifiedOn"]?.ToString(),
                Status = source["Status"]?.ToString(),
                OrderId = source["OrderId"]?.ToString(),
                Id = source["id"]?.ToString(),
                PartitionKey = source["PartitionKey"]?.ToString(),
                Pos = source["Pos"] != null ? (Pos)source["Pos"] : null,
                Parties = source["Parties"] != null ? (Parties)source["Parties"] : null,
                BookingReferences = source["BookingReferences"] != null ? (BookingReference)source["BookingReferences"] : null,
                PaymentDetails = source["PaymentDetails"]?.Select(a => (RefundPayment)a),
                AccountableDocumentRefs = source["AccountableDocumentRefs"]?.Select(a => (AccountableDocumentRef)a),
            };
        }

    }
}
