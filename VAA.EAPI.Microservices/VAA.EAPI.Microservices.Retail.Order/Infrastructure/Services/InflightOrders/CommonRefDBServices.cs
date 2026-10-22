using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.CommonRefDB.Models;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders
{
    public class CommonRefDBServices : ICommonRefDBServices
    {
        private readonly CommonRefDbContext _commonRefDbContext;

        public CommonRefDBServices(CommonRefDbContext commonRefDbContext)
        {
            _commonRefDbContext = commonRefDbContext;
        }
        public IEnumerable<DimAirport> GetAllAirports()
        {
            IEnumerable<DimAirport> airportData = _commonRefDbContext.DimAirports.AsEnumerable();
            return airportData;
        }
        public IEnumerable<DimCarrier> GetAllCarriers()
        {
            IEnumerable<DimCarrier> carrierData = _commonRefDbContext.DimCarriers.AsEnumerable();
            return carrierData;
        }
    }
}
