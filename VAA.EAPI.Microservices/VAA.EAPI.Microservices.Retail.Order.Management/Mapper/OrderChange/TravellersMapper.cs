using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class TravellersMapper
    {
        public static Traveller Mapper(IEnumerable<CustomerDetails> customers, CustomerDetails passenger)
        {
            var traveller = customers.First(x => string.Equals(x.FirstName, passenger.FirstName, StringComparison.OrdinalIgnoreCase) && string.Equals(x.LastName, passenger.LastName, StringComparison.OrdinalIgnoreCase));
            return new Traveller()
            {
                FirstNameNumber = traveller.FirstNameNumber,
                LastNameNumber = traveller.LastNameNumber,
            };
        }
    }
}
