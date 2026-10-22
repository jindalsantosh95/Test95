using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public class OrderSeatFulfilmentCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(SeatFulfilmentResponse seatFulfilmentResponse, string id)
        {
            return new CloudEvent()
            {
                Id = id,
                Data = seatFulfilmentResponse,
                Type = Constants.Application.OrderChangeEventSubject,
                Source = ConfigReader.Get(Constants.Environment.OrderSeatServiceBusTopicName),
                Subject = Constants.Application.OrderChangeEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
