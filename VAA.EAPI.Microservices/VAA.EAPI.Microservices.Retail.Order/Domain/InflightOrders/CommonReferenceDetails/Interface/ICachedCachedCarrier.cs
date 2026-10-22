using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Models;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.InflightOrders.CommonReferenceDetails.Interface
{
    public interface ICachedCarrier
    {
        List<CarrierDetails> GetCachedCarrier();
    }

}
