using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.CommonRefDB;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Model;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper.CommonRefDetailsMapper
{
    public class CarrierDataMapper
    {
        public static List<CarrierDetails> MapCarrierData(IEnumerable<DimCarrier> carriers)
        {
            List<CarrierDetails> carierDetailsList = new List<CarrierDetails>();
            foreach (var m in carriers)
            {
                CarrierDetails carrier = new CarrierDetails()
                {
                    AirlineName = m?.AirlineName,
                    IataAlphaCode = m.IataAlphaCode
                };
                carierDetailsList.Add(carrier);
            }
            return carierDetailsList;
        }
    }
}
