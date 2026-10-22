using System;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class CancelPnrEventMapper
    {
        public static CloudEvent FailureEventMapper(BookingReference bookingRef, string correlationId)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Data = bookingRef,
                Source = Constants.Application.NDCCancelPnrHandlerTopicName,
                Time = DateTime.Now.ToString(Constants.Application.ZuluDateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
