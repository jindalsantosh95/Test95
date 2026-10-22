using MediatR;
using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction
{
    public class CreateTransactionRequestCommand : IRequest<Result<Response<string>>>
    {
        public CreateTransactionRequest Request { get; set; }
    }
}