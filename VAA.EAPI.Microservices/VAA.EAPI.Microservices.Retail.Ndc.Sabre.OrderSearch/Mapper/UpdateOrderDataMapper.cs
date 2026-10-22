using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Mapper
{
    public class UpdateOrderDataMapper
    {
      
        public static CloudEvent EventMapper(OrderDocument request, string correlationId)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Data = request,
                Type = Constants.Application.UpdateOrderEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
            };
        }

        public static Response<string> ResponseMapper(string correlationid)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
            };
        }

    }
}
