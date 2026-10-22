using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate
{
    public class ARCDocument : CosmosDocument
    {
        public BookingReference BookingReference { get; set; }
        public IEnumerable<ARCTransactionHistory> ARCTransactionHistory { get; set; }
    }

}
