using System;
using CloudEvent = VAA.EAPI.Domain.Events.CloudEvent;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Mapper
{
    public class UpdateTicketCloudEventMapper
    {
        public static CloudEvent EventMapper(IssueTicketResponse response)
        {
            return new CloudEvent()
            {
                Data = response,
                Source = Constants.Application.NDCUpdateTicketTopicName,
                Time = DateTime.Now.ToString(Constants.Application.ZuluDateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }

        public static CloudEvent FailureEventMapper(IssueTicketResponse response)
        {
            return new CloudEvent()
            {
                Data = (PnrData)response,
                Source = Constants.Application.NDCCancelPnrTopicName,
                Time = DateTime.Now.ToString(Constants.Application.ZuluDateTimeFormat),
                Specversion = Constants.Application.EventSpecversion,
                DataContentType = Constants.Application.EventDataContentType
            };
        }
    }
}
