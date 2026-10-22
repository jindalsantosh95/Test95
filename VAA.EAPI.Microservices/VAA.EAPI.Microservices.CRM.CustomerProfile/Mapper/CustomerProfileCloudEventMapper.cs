using Microsoft.Azure.Amqp.Framing;
using System;
using System.Collections.Generic;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Mapper
{
    public static class CustomerProfileCloudEventMapper
    {
        public static CloudEvent EventMapper(List<FlightOrderData> request, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = request,
                Type = Constants.Constants.CheckinCustProfileResEventSubject,
                Source = ConfigReader.Get(Constants.Environment.CheckinNotificationTopicName),                 
                Subject = Constants.Constants.CheckinCustProfileResEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
                
            AsyncContext.Current.MapToCloudEvent(cloudEvent);

            return cloudEvent;
        }

    }
}
