using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Mapper.InflightOrders.CommonRefDetailsMapper;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders.Interface;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.CommonRefDB.Models;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders
{
    public class CommonRefDBRepository : ICommonRefDBRepository
    {
        private readonly ICommonRefDBServices _commonRefDBServices;
        private readonly ILogger<CommonRefDBRepository> _logger;

        public CommonRefDBRepository(ICommonRefDBServices commonRefDBServices, ILogger<CommonRefDBRepository> logger)
        {
            _logger = logger;
            _commonRefDBServices = commonRefDBServices;
        }
        public List<AirportDetails> GetAllAirportDetails()
        {
            IEnumerable<DimAirport> airportData = _commonRefDBServices.GetAllAirports();
            _logger.LogInformation($"Airport data fetched successfully");
            List<AirportDetails> airportDetails = AirportDataMapper.MapAirportData(airportData);
            return airportDetails;

        }
        public List<CarrierDetails> GetAllCarrierDetails()
        {
            IEnumerable<DimCarrier> carrierData = _commonRefDBServices.GetAllCarriers();
            _logger.LogInformation($"Carrier data fetched successfully");
            List<CarrierDetails> carrierDetails = CarrierDataMapper.MapCarrierData(carrierData);
            return carrierDetails;
        }

    }
}
