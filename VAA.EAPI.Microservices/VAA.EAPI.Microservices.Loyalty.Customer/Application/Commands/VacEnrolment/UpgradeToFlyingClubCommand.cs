using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class UpgradeToFlyingClubCommand : IRequest<Response<string>>
    {
        public CreateCustomerEntities CustomerDetails { get; set; }
    }
}
