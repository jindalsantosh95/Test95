using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Model;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Interface
{
    public interface ISCVRefDBService
    {
        public Task<IEnumerable<Orders>> GetCustomerOrders(BookingQuery query);
    }
}
