using System;
using System.Collections.Generic;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using Orderdata = VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs;
using FlightOrderData = VAA.EAPI.Orchestration.Notifications.Domain.Entities.FlightOrderData;

namespace VAA.EAPI.Orchestration.Notifications.Mapper
{
    public static class NotifCloudEventMapper
    {
        public static CloudEvent FlightOrderDataEventMapper(FlightOrderDataRequest request, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = request,
                Type = Constants.Constants.FlightOrderDataReqEventSubject,
                Source = ConfigReader.Get(Constants.Environment.NotificationTopicName),
                Subject = Constants.Constants.FlightOrderDataReqEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            return cloudEvent;
        }

        public static CloudEvent CustomerProfileEventMapper(List<Orderdata.FlightOrderData> request, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = request,
                Type = Constants.Constants.CheckinFlightCustProfileReqEventSubject,
                Source = ConfigReader.Get(Constants.Environment.NotificationTopicName),
                Subject = Constants.Constants.CheckinFlightCustProfileReqEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            return cloudEvent;
        }

        public static CloudEvent FlightOrderNotifEventMapper(List<Orderdata.FlightOrderData> request, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = Json.Serialize(request).Compress(),
                Type = Constants.Constants.FlightOrderNotifReqEventSubject,
                Source = ConfigReader.Get(Constants.Environment.NotificationTopicName),
                Subject = Constants.Constants.FlightOrderNotifReqEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            return cloudEvent;
        }

        public static CloudEvent NotifEventMapper(NotificationRequest request, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = Json.Serialize(request).Compress(),
                Type = Constants.Constants.FlightOrderNotifReqEventSubject,
                Source = ConfigReader.Get(Constants.Environment.NotificationTopicName),
                Subject = Constants.Constants.FlightOrderNotifReqEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            return cloudEvent;
        }

        public static CloudEvent AircraftChangeNotifEventMapper(IEnumerable<FlightOrderData> request, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = Json.Serialize(request).Compress(),
                Type = Constants.Constants.AircraftChangeNotifReqEventSubject,
                Source = ConfigReader.Get(Constants.Environment.NotificationTopicName),
                Subject = Constants.Constants.AircraftChangeNotifReqEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            return cloudEvent;
        }

    }
}
