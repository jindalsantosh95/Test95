using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class WebMYACommandRequestMapper
    {
        public static WebMYACommand EventRequestMapper(CloudEvent customerRequest)
        {
            return new WebMYACommand
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
