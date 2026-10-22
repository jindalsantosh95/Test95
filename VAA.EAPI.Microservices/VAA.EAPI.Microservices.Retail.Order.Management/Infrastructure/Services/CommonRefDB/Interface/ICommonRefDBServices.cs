using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Model;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.CommonRefDB.Interface
{
    public interface ICommonRefDBServices
    {
        public IEnumerable<DimAirport> GetAllAirports();
        public IEnumerable<DimCarrier> GetAllCarriers();
    }
}
