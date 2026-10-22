using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier
{
    public class CheckEmailAndTierQuery : IRequest<bool>
    {
        public EnrolmentRequestCommand EnrolmentRequest { get; set; }
        public CreateCustomerEntities CreateCustomerEntities { get; set; }
        public CheckEmailAndTierQuery(EnrolmentRequestCommand enrolmentRequest, CreateCustomerEntities createCustomerEventEntities)
        {
            EnrolmentRequest = enrolmentRequest;
            CreateCustomerEntities = createCustomerEventEntities;
        }
    }
}
