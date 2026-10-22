using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class CustomerCommandRequestMapper
    {
        public static CustomerCommand EventRequestMapper(CloudEvent customerRequest)
        {
            return new CustomerCommand
            {
                Id = customerRequest.Id,
                Data = customerRequest.Data,
                Type = customerRequest.Type,
                Subject = customerRequest.Subject,
                Specversion = customerRequest.Specversion
            };
        }
    }
}
