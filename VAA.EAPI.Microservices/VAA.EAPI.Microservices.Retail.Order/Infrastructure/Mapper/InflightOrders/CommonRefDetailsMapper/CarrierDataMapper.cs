using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.InflightOrders.CommonRefDB.Models;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Mapper.InflightOrders.CommonRefDetailsMapper
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
