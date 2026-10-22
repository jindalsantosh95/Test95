using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatDeletionCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(SeatDeletionResponse seatDelResponse, string id, string source)
        {
            return new CloudEvent()
            {
                Id = id,
                Data = seatDelResponse,
                Source = source,
                Type = Constants.Application.OrderChangeEventSubject,
                Subject = Constants.Application.OrderChangeEventSubject,
                Time = DateTime.Now.ToString(Constants.Application.ZuluDateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
