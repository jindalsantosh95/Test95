using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class BasicEnrolmentMapper
    {
        public static BasicCustomerCommand BasicEventMapper(CloudEvent request)
        {
            return new BasicCustomerCommand
            {
                Id = request.Id,
                Subject = request.Subject,
                Type = request.Type,
                Data = request.Data,
                Source = request.Source,
                Specversion = request.Specversion
            };
        }
    }
}
