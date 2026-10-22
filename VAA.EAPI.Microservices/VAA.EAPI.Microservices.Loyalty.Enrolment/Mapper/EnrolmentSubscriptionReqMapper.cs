using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper
{
    class EnrolmentSubscriptionReqMapper
    {
        public static EnrolmentEventProcessCommand EnrolmentEventRequestMapper(CloudEvent enrolmentRequestEventMessage)
        {
            return new EnrolmentEventProcessCommand
            {
                Id = enrolmentRequestEventMessage.Id,
                Data = enrolmentRequestEventMessage.Data,
                Type = enrolmentRequestEventMessage.Type,
                Subject = enrolmentRequestEventMessage.Subject,
                Specversion = enrolmentRequestEventMessage.Specversion
            };
        }
    }
}
