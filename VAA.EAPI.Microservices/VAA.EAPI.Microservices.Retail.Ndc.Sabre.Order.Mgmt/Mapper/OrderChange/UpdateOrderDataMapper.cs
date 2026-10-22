using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public class UpdateOrderDataMapper
    {

        public static CloudEvent EventMapper(OrderDocument request, string id)
        {
            return new CloudEvent()
            {
                Id = id,
                Data = request,
                Type = Constants.Application.UpdateOrderEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
            };
        }

        public static Response<string> ResponseMapper()
        {
            return new Response<string>
            {
                Status = Status.Accepted,
            };
        }

    }
}
