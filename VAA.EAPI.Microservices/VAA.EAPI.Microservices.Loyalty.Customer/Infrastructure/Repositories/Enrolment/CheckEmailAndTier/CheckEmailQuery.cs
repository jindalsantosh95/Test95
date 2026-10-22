using MediatR;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier
{
    public class CheckEmailQuery : IRequest<VacEmailCheckResponse>
    {
        public string Email { get; set; }
    }
}
