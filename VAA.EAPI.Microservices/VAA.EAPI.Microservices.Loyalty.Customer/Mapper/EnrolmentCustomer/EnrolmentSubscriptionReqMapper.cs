using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class EnrolmentSubscriptionReqMapper
    {
        public static EnrolmentEventProcessCommand EnrolmentEventRequestMapper(EAPI.Domain.Events.CloudEvent enrolmentRequestEventMessage)
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
