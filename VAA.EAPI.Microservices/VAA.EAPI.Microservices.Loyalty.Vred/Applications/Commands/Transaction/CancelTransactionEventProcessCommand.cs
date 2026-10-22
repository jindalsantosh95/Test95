using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction
{
    public class CancelTransactionEventProcessCommand : CloudEvent, IRequest<Result<Response<string>>>
    {
    }
}