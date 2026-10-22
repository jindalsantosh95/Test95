using System.Collections.Generic;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Notification.Constants;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Mapper
{
    public class OrderNotificationStatusResponseMapper
    {
        public static Response<OrderNotificationStatusResponse> ResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<OrderNotificationStatusResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<OrderNotificationStatusResponse>(serviceResponse.JsonResponseData)
            };
        }
        public static Response<OrderNotificationStatusResponse> MapToOrderNotificationStatusResponse(string correlationid, Status status)
        {
            return new Response<OrderNotificationStatusResponse>
            {
                Status = status,
                HeaderParameters = new List<ResponseHeader>
                {
                    new ResponseHeader
                    {
                        Key = Environment.HeaaderParameterlocation,
                        Value = ConfigReader.Get(Environment.OrderNotificationResponseUrl).Replace("{correlationid}", correlationid)
                    },
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
