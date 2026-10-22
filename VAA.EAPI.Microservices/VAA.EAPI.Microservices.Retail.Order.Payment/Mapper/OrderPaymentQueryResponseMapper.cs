using System.Collections.Generic;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Constants;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper
{
    public class OrderPaymentQueryResponseMapper
    {
        public static Response<OrderPaymentResponse> OrderPaymentStatusQueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<OrderPaymentResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<OrderPaymentResponse>(serviceResponse.JsonResponseData)
            };
        }

        public static Response<OrderPaymentResponse> OrderPaymentStatusResponseMapper(string correlationid, Status status)
        {
            return new Response<OrderPaymentResponse>
            {
                Status = status,
                HeaderParameters = new List<ResponseHeader>
                { new ResponseHeader
                    {
                        Key = Environment.HeaderParameterlocation,
                        Value = ConfigReader.Get(Environment.OrderPaymentResponseUrl).Replace("{correlationid}", correlationid) },
                  new ResponseHeader
                  {
                      Key = Environment.HeaderRetryAfter,
                      Value = ConfigReader.Get(Environment.HeaderPaymentStatusRetryAfterValue)
                  }
                }
            };
        }
    }
}
