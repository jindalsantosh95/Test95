using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class VredOcdbSyncEventMapper
    {
        public static CloudEvent EventDataMapper(string eventId, CustomerAccountDetails customerDetails)
        {
            if (customerDetails == null) throw new ArgumentNullException(nameof(customerDetails));

            return new CloudEvent
            {
                Id = eventId,
                Type = Constants.Application.VRedOcDbSyncEventType,
                Data = MapCustomerDetailsToCustomerEntities.MaptoCreateCustomerEntities(customerDetails),
                Subject = Constants.Application.CustomerSyncEventSubject,
                Source = ConfigReader.Get(Environment.CustomerSyncEndPoint),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.EventSpecVersion
            };
        }
    }
}


