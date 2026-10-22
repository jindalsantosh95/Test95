using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.InsurancePurchase
{
    public class InsurancePurchaseEventCommand : CloudEvent, IRequest<Result<Unit>>
    {
       
    }
}