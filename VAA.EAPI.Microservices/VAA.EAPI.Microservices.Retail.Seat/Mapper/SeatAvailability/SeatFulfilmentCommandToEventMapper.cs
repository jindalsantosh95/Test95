using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatFulfilmentCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(SeatFulfilmentResponse seatFulfilmentResponse, string id)
        {
            return new CloudEvent()
            {
                Id = id,
                Data = seatFulfilmentResponse,
                Source = Constants.Application.NDCUpdateSeatTopicName,
                Time = DateTime.Now.ToString(Constants.Application.ZuluDateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
