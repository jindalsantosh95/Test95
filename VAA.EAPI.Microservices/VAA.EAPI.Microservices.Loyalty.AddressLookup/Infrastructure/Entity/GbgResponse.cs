using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity
{
    public class GbgResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
    }   
}
