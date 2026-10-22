using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.CommonRefDB;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.CommonRefDB.Interface
{
    public interface ICommonRefDBRepository
    {
        public List<AirportDetails> GetAllAirportDetails();
        public List<CarrierDetails> GetAllCarrierDetails();

    }
}
