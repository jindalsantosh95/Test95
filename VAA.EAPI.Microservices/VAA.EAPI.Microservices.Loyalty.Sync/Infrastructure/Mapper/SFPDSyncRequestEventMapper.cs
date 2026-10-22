using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;
using Environment = VAA.EAPI.Microservices.Loyalty.Sync.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper
{
    public class SFPDSyncRequestEventMapper
    {
        public static CloudEvent CommandToEventMapper(LoyaltySyncRequestData request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            
            return new CloudEvent()
            {
                Data = request,
                Type = ConfigReader.Get(Environment.EventTypePSDSSync),//CustomerSync
                Source = ConfigReader.Get(Environment.ServiceBusTopicName),//enrolcustomer
                Subject = ConfigReader.Get(Environment.EventTypePSDSSync), //Constants.Application.CreateCustomerEventSubject,//Enrolment
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion 
            };
        }
    }
}
