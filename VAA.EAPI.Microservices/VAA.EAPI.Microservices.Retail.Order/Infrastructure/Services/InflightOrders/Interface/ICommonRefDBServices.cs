using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.Interface
{
    public interface ICommonRefDBServices
    {
        public IEnumerable<DimAirport> GetAllAirports();
        public IEnumerable<DimCarrier> GetAllCarriers();
    }
}
