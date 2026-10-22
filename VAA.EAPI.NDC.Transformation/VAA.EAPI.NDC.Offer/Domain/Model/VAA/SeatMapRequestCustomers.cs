using System.Collections.Generic;
using VAA.EAPI.NDC.Offer.Domain.Model.Order;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{

    public class SeatMapRequestCustomers
    {
        public IEnumerable<Customer> KnownCustomers { get; set; }

        public IEnumerable<AnonymousCustomer> AnonymousCustomers { get; set; }

    }
}
