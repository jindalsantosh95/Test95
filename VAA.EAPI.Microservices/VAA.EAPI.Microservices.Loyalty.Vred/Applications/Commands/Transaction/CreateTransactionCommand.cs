using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction
{
    public class CreateTransactionCommand : IRequest<VRedTransactionResponse>
    {
        public string Id { get; set; }
        public CreateTransactionRequest Request { get; set; }
    }
}