using MediatR;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Insurance.Domain.Decision;


namespace VAA.EAPI.Microservices.Retail.Insurance.Application.Commands
{
    public class DecisionCommand : CloudEvent, IRequest<Unit>
    {
        public DecisionRequest Request { get; set; }

    }
}
