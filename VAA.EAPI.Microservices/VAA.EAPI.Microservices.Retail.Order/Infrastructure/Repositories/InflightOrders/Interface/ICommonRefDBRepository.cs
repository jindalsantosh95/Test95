using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders.Interface
{
    public interface ICommonRefDBRepository
    {

        public List<AirportDetails> GetAllAirportDetails();
        public List<CarrierDetails> GetAllCarrierDetails();
    }
}
