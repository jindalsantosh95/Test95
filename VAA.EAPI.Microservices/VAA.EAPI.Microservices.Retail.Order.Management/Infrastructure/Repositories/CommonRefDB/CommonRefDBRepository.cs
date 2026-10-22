using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.CommonRefDB;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper.CommonRefDetailsMapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Model;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.CommonRefDB.Interface;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.CommonRefDB.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.CommonRefDB
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
            _logger.LogDebug($"Airport data fetched successfully");
            List<AirportDetails> airportDetails = AirportDataMapper.MapAirportData(airportData);
            return airportDetails;

        }
        public List<CarrierDetails> GetAllCarrierDetails()
        {
            IEnumerable<DimCarrier> carrierData = _commonRefDBServices.GetAllCarriers();
            _logger.LogDebug($"Carrier data fetched successfully");
            List<CarrierDetails> carrierDetails = CarrierDataMapper.MapCarrierData(carrierData);
            return carrierDetails;
        }

    }
}
