using MediatR;
using VAA.EAPI.Microservices.Retail.Insurance.Domain.Decision;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Services.OrderCreate
{
    public class ServiceDecisionCommand : IRequest<Unit>
    {
        public DecisionRequest Request { get; set; }
    }
}
