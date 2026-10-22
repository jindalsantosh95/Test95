using MediatR;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CheckEmailAndTier
{
    public class CheckEmailAndTierQuery : IRequest<bool>
    {
        public EnrolmentRequest EnrolmentRequest { get; set; }
        public CreateCustomerEventEntities CreateCustomerEntities { get; set; }
        public CheckEmailAndTierQuery(EnrolmentRequest enrolmentRequest, CreateCustomerEventEntities createCustomerEventEntities)
        {
            EnrolmentRequest = enrolmentRequest;
            CreateCustomerEntities = createCustomerEventEntities;
        }
    }
}
