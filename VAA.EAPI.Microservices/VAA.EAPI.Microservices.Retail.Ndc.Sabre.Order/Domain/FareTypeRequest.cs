using System;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain
{
    public class FareTypeRequest
    {
        public string FareTypeCode { get; set; }
        public string FareSubTypeCode { get; set; }

        public static explicit operator FareTypeRequest(OrderCreateRequest orderCreateRequest)
        {
            return new FareTypeRequest
            {
                FareTypeCode = orderCreateRequest?.FareTypeRequest?.FareTypeCode,
                FareSubTypeCode = orderCreateRequest?.FareTypeRequest?.FareSubTypeCode
            };
        }
    }
}