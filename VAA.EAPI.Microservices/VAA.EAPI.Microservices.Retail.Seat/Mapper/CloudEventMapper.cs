using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Seats;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Domain;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper
{
    internal class CloudEventMapper
    {
        public static CloudEvent Map(SeatDeletionRequest requestData)
        {           
            var cloudEvent = new CloudEvent()
            {
                Type = EnumOperations.GetEnumDescription((SeatFulfilmentEventType)Enum.Parse(typeof(SeatFulfilmentEventType), SeatFulfilmentEventType.SeatDeletionInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.SeatDeletionServiceBusTopicName),
                Data = requestData,
                Subject = Constants.Application.SeatDeletionEventSubject,
                Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }

        public static CloudEvent Map(SeatFulfilmentRequest seatFulfilmentRequest)
        {
            var cloudEvent = new CloudEvent()
            {
                Data = seatFulfilmentRequest,
                Type = EnumOperations.GetEnumDescription((SeatFulfilmentEventType)Enum.Parse(typeof(SeatFulfilmentEventType), SeatFulfilmentEventType.SeatFulfilmentInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.SeatFulfilmentEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;

        }

        public static CloudEvent Map(OrderEmailRequest orderEmailRequest)
        {
            var cloudEvent = new CloudEvent()
            {
                Data = orderEmailRequest,
                Type = EnumOperations.GetEnumDescription((SeatFulfilmentEventType)Enum.Parse(typeof(SeatFulfilmentEventType), SeatFulfilmentEventType.SeatPaymentReceiptEmailTrigger.ToString())),
                Source = ConfigReader.Try(Constants.Environment.EmailMSServiceBusTopicName),
                Subject = Constants.Application.SeatPaymentReceiptEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }
    }
}
