using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile
{
    public class CustomerProfileDocResponse : ICustomerTransactionQueryResponse
    {
        public CustomerProfile CustomerProfile { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public CustomerProfileDocResponse()
        {
            // Required for Serialisation
        }

        public CustomerProfileDocResponse(string customerId, IEnumerable<object> documents)
        {
            CustomerId = customerId;
            IEnumerable<CustomerProfileDTO> dtoList = Json.Deserialize<IEnumerable<CustomerProfileDTO>>(Json.Serialize(documents));
            CustomerProfile = dtoList.Select(dto => new CustomerProfile(dto)).FirstOrDefault();
        }
    }
}